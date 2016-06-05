using Java.Nio.Channels;
using Java.Net;
using Java.Nio;
using System.Collections.Generic;
using System.Security.Cryptography;
using Android.Widget;
using Android.App;
using Android.Appwidget;
using System.Linq;
using System.Numerics;
namespace TINClient
{

    public class LogicLayer
    {
        SelectionKey comPipeKey;
        SelectionKey intPipeKey;
        public LogicLayer()
        {
            
            
        }
        public void Run()
        {
            try
            {
                Selector selector = Selector.Open();

                Pipe.SourceChannel intPipeSource = Model.interruptPipe.Source();
                intPipeSource.ConfigureBlocking(false);
                intPipeKey = intPipeSource.Register(selector, Operations.Read);


                Pipe.SourceChannel comPipeSource = Model.communicationPipe.Source();
                comPipeSource.ConfigureBlocking(false);
                comPipeKey = comPipeSource.Register(selector, Operations.Read);



                // shouldent really be here - it's same as lower
                selector.Select();
                ICollection<SelectionKey> selectedKeys = selector.SelectedKeys();


                foreach (SelectionKey key in selectedKeys)
                {
                    if (key == comPipeKey)
                    {
                        ByteBuffer signal = ByteBuffer.Allocate(1);
                        comPipeSource.Read(signal);
                        signal.Flip();
                        byte signalByte = (byte)signal.Get();
                        if (signalByte == (byte)Signal.Connect)
                            break;
                        else
                            throw new System.Exception("wrong");
                    }
                }







                securityLayer = new SecurityLayer();
                if(Model.mainActivity!=null)
                  Model.mainActivity.Output("connected?");
                /*
                while (true)
                {
                    



                    selector.Select();
                    selectedKeys = selector.SelectedKeys();
                    foreach (SelectionKey key in selectedKeys)
                    {
                       if (key == intPipeKey)
                           throw new System.Exception("pipe");
                    }
                    foreach (SelectionKey key in selectedKeys)
                    {
                        if (key == comPipeKey)
                        {
                            ByteBuffer signal = ByteBuffer.Allocate(1);
                            comPipeSource.Read(signal);
                            signal.Flip();
                            byte signalByte = (byte)signal.Get();
                            if (signalByte == (byte)Signal.Send)
                            {
                                byte[] data = new byte[19];
                                for (byte i = 0; i < 19; i++)
                                {
                                    data[i] = (byte)(0x61 + i);
                                }
                                securityLayer.Send(data);
                            }
                            if (signalByte == (byte)Signal.Recive)
                            {
                                List<byte> output = securityLayer.Recive();
                                Model.mainActivity.Output(System.Text.Encoding.ASCII.GetString(output.ToArray()));
                            }
                        }
                    }
                    
                }*/
                SignIn();
            }
            catch (System.Exception e)
            {
                string exception = e.Message;
                Disconnect();
            }


            




        }

        void Disconnect()
        {
            securityLayer = null;
            Model.DestroyConnection();
            
        }

        void SignIn()
        {
            SendFrame(0, Model.username);//or + \0
            byte[] message;
            if(ReciveFrame(out message) !=1)
                throw (new System.Exception("wrong message type"));


            byte[] responseValue = new byte[Model.password.Length + message.Length];
            Model.password.CopyTo(responseValue, 0);
            message.CopyTo(responseValue, Model.password.Length);

            MD5 md5 = MD5.Create();
            byte[] response = md5.ComputeHash(responseValue);

            SendFrame(2, response);

            if (ReciveFrame(out message) != 3)
                throw (new System.Exception("unable to login"));
        }

        void SendFrame(byte messageType,byte[] message)///should really be optimized  not to copy frame over again
        {
            byte[] frame = new byte[1 + message.Length];
            frame[0] = messageType;
            message.CopyTo(frame, 1);
            securityLayer.Send(frame);
        }

        int ReciveFrame(out byte[] message)///returns - messagetype //to change drasticly
        {
            List<byte> frame = securityLayer.Recive();

            int messageType = frame[0];
            frame.RemoveAt(0);


            message = frame.ToArray();
            return messageType;

        }

        SecurityLayer securityLayer;
    }


    internal class SecurityLayer
    {
        TCPLayer tcpLayer;
       // byte[] serverRSA;
        ICryptoTransform symmetricEncryptor;
        ICryptoTransform symmetricDecryptor;
        Aes sessionCypher;
        public SecurityLayer()
        {
            tcpLayer = new TCPLayer();

            tcpLayer.Send(new byte[1] {0});

           // serverRSA = new byte[128];
          //  data.CopyTo(1,serverRSA,0,128);

            List<byte> modulusData= tcpLayer.Recive();
            List<byte> publicKeyData = tcpLayer.Recive();

            if (modulusData[0] != 1)
                throw (new System.Exception("security"));
            modulusData.RemoveAt(0);

            if (publicKeyData[0] != 1)
                throw (new System.Exception("security"));
            publicKeyData.RemoveAt(0);


            RSAParameters rsaKey = new RSAParameters();

            rsaKey.Modulus = modulusData.ToArray();
            rsaKey.Exponent = publicKeyData.ToArray();

            RSACryptoServiceProvider rsaCypher = new RSACryptoServiceProvider();

            rsaCypher.ImportParameters(rsaKey);

            sessionCypher=AesCryptoServiceProvider.Create();
            sessionCypher.Padding = PaddingMode.PKCS7;
            sessionCypher.Mode = CipherMode.ECB;
            sessionCypher.GenerateKey();
            symmetricEncryptor = sessionCypher.CreateEncryptor();
            symmetricDecryptor = sessionCypher.CreateDecryptor();

            byte[] encryptedKey = rsaCypher.Encrypt(sessionCypher.Key, false);


            byte[] frameToSend;
            frameToSend = new byte[1 + encryptedKey.Length];
            frameToSend[0] = 2;
            encryptedKey.CopyTo(frameToSend, 1);

            tcpLayer.Send(frameToSend);

        }
        public void Send(byte[] message)
        {

             byte[] encryptedMessage = new byte[((message.Length+15)/16)*16 + 1];
             encryptedMessage[0] = 3;
             symmetricEncryptor.TransformFinalBlock(message, 0, message.Length).CopyTo(encryptedMessage, 1);
             tcpLayer.Send(encryptedMessage);
         }

        public List<byte> Recive()
        {
            List<byte> message = tcpLayer.Recive();
            if (message[0] != 3)
                throw (new System.Exception("security"));
            message.RemoveAt(0);
            byte[] array = message.ToArray();
            byte[] decryptedMessage=symmetricDecryptor.TransformFinalBlock(array, 0, message.Count);
            List<byte> returned = new List<byte>();
            returned.AddRange(decryptedMessage);
            return returned;
        }

    }


    internal unsafe class TCPLayer
    {
        //  unsafe struct Header
        //  {
        //      public int flag;
        //       public int lenght;
        //      public byte[16] checksum;
        //  }
        SocketChannel socket;
        //   Selector selector;
        //   SelectionKey socketKey;
        
        public TCPLayer()
        {
            SelectionKey pipeKey;
            Selector selector=Selector.Open();
            SelectionKey socketKey;
            socket = SocketChannel.Open();
            socket.ConfigureBlocking(false);
            socketKey = socket.Register(selector, Operations.Connect);

            pipeKey = Model.interruptPipeSource.Register(selector,Operations.Read);
            if (socket.Connect(Model.serwerAddress) == false)
            {
                selector.Select();
                ICollection<SelectionKey> selectedKeys = selector.SelectedKeys();
                foreach(SelectionKey key in selectedKeys)
                {
                    if (key == pipeKey)
                        throw new System.Exception("pipe");
                }
                socket.FinishConnect();
            }



        }
        ~TCPLayer()
        {
            socket.Dispose();
        }
        public void Send(byte[] message)
        {
            int sent = 0;
            while(sent<message.Length)
            {
                if(message.Length-sent>Model.FrameSize)
                {
                    SendFrame(message, ByteBuffer.Wrap(message,sent,Model.FrameSize), 0);
                    sent += Model.FrameSize;
                }
                else
                {
                    SendFrame(message, ByteBuffer.Wrap(message, sent, message.Length - sent), 1);
                    sent = message.Length;
                }
                if (Model.mainActivity != null)
                    Model.mainActivity.Output("sent " + sent + " bytes");
                System.Threading.Thread.Sleep(1000);
            }
        }

        void SendFrame(byte[] message,ByteBuffer messageBuffer,int flag)
        {
            SelectionKey pipeKey;
            Selector selector = Selector.Open();
            SelectionKey socketKey = socket.Register(selector, Operations.Write);
            pipeKey = Model.interruptPipe.Source().Register(selector, Operations.Read);
            messageBuffer.Mark();
            ByteBuffer header = ByteBuffer.Allocate(24);
            header.Order(ByteOrder.BigEndian);
            // so baaad
            header.PutInt(flag);
            header.PutInt(messageBuffer.Remaining());
            MD5 hash = MD5.Create()  ;
            byte[] hashvalue = hash.ComputeHash(message,messageBuffer.ArrayOffset(),messageBuffer.Capacity());
            for (int i = 0; i < 16; i++)
                header.Put((sbyte)hashvalue[i]);
            header.Rewind();
            while (header.HasRemaining) //send header
            {
                selector.Select();
                ICollection<SelectionKey> selectedKeys = selector.SelectedKeys();
                foreach (SelectionKey key in selectedKeys)
                    if(key == pipeKey)
                        throw new System.Exception("pipe");
                socket.Write(header);
            }
            // ByteBuffer sendingBuff = ByteBuffer.Wrap(message);
            messageBuffer.Reset();
            while (messageBuffer.HasRemaining) //send payload
            {
                selector.Select();
                ICollection<SelectionKey> selectedKeys = selector.SelectedKeys();
                foreach (SelectionKey key in selectedKeys)
                    if (key == pipeKey)
                        throw new System.Exception("pipe");
                socket.Write(messageBuffer);
            }
        }


        public List<byte> Recive()
        {
            List<byte> returned = new List<byte>();
            KeyValuePair<int, byte[]>  recived = ReciveFrame();
            int flag = recived.Key;
            returned.AddRange(recived.Value);
            while (flag==0)
            {
                recived = ReciveFrame();
                returned.AddRange(recived.Value);
                flag = recived.Key;
            }
            return returned;
        }

        KeyValuePair<int,byte[]> ReciveFrame()
        {
            Selector selector = Selector.Open();
            SelectionKey pipeKey = Model.interruptPipeSource.Register(selector, Operations.Read); ;
            SelectionKey socketKey = socket.Register(selector, Operations.Read);

            ByteBuffer header = ByteBuffer.Allocate(24);

            while (header.HasRemaining) //recive header
            {
                selector.Select();
                ICollection<SelectionKey> selectedKeys = selector.SelectedKeys();
                foreach (SelectionKey key in selectedKeys)
                    if (key == pipeKey)
                        throw new System.Exception("pipe");
                socket.Read(header);
            }
            header.Rewind();
            int flag = header.Int;
            int length = header.Int;
            byte[] hash = new byte[16];
            header.Get(hash);

            byte[] messageFrame = new byte[length];
            ByteBuffer messageFrameBuf = ByteBuffer.Allocate(length);

            while (messageFrameBuf.HasRemaining) //recive message
            {
                selector.Select();
                ICollection<SelectionKey> selectedKeys = selector.SelectedKeys();
                foreach (SelectionKey key in selectedKeys)
                    if (key == pipeKey)
                        throw new System.Exception("pipe");
                socket.Read(messageFrameBuf);
            }
            messageFrameBuf.Rewind();
            messageFrameBuf.Get(messageFrame);
            MD5 md5 = MD5.Create();
            byte[] targetHash=md5.ComputeHash(messageFrame);
            for (int i = 0; i < 6; i++)
                if (targetHash[i] != hash[i]) ;//for now
                    //throw new System.Exception("checksum");

            return new KeyValuePair<int, byte[]>(flag, messageFrame);
        }

    }

}