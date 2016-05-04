using Java.Nio.Channels;
using Java.Net;
using Java.Nio;
using System.Collections.Generic;
using System.Security.Cryptography;
using Android.Widget;
using Android.App;
using Android.Appwidget;
using System.Linq;
namespace TINClient
{

    internal class LogicLayer
    {
        SelectionKey comPipeKey;
        SelectionKey intPipeKey;
        public LogicLayer(Model m)
        {
            model = m;
            
        }
        public void Run()
        {
            try
            {
                securityLayer = new SecurityLayer(model);
                model.mainActivity.Output("connected?");
                while (true)
                {
                    Selector selector = Selector.Open();

                    Pipe.SourceChannel intPipeSource = model.interruptPipe.Source();
                    intPipeSource.ConfigureBlocking(false);
                    intPipeKey = intPipeSource.Register(selector, Operations.Read);


                    Pipe.SourceChannel comPipeSource = model.communicationPipe.Source();
                    comPipeSource.ConfigureBlocking(false);
                    comPipeKey = comPipeSource.Register(selector, Operations.Read);



                    selector.Select();
                    ICollection<SelectionKey> selectedKeys = selector.SelectedKeys();
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
                                model.mainActivity.Output(System.Text.Encoding.ASCII.GetString(output.ToArray()));
                            }
                        }
                    }
                    
                }
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
            model.DestroyConnection();
            
        }

        Model model;
        SecurityLayer securityLayer;
    }


    internal class SecurityLayer
    {
        Model model;
        TCPLayer tcpLayer;
        public SecurityLayer(Model m)
        {
            model = m;
            tcpLayer = new TCPLayer(model);
        }
        public void Send(byte[] message)
        {
            tcpLayer.Send(message);
        }

        public List<byte> Recive()
        {
            return tcpLayer.Recive();
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
        Model model;
        SocketChannel socket;
        //   Selector selector;
        //   SelectionKey socketKey;
        
        public TCPLayer(Model m)
        {
            model = m;
            SelectionKey pipeKey;
            Selector selector=Selector.Open();
            SelectionKey socketKey;
            socket = SocketChannel.Open();
            socket.ConfigureBlocking(false);
            socketKey = socket.Register(selector, Operations.Connect);

            pipeKey = model.interruptPipeSource.Register(selector,Operations.Read);
            if (socket.Connect(model.serwerAddress) == false)
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
                if(message.Length-sent>model.FrameSize)
                {
                    SendFrame(message, ByteBuffer.Wrap(message,sent,model.FrameSize), 0);
                    sent += model.FrameSize;
                }
                else
                {
                    SendFrame(message, ByteBuffer.Wrap(message, sent, message.Length - sent), 1);
                    sent = message.Length;
                }
                model.mainActivity.Output("sent " + sent + " bytes");
                System.Threading.Thread.Sleep(1000);
            }

            SendFrame(message,ByteBuffer.Wrap(message),1);
        }

        void SendFrame(byte[] message,ByteBuffer messageBuffer,int flag)
        {
            SelectionKey pipeKey;
            Selector selector = Selector.Open();
            SelectionKey socketKey = socket.Register(selector, Operations.Write);
            pipeKey = model.interruptPipe.Source().Register(selector, Operations.Read);

            ByteBuffer header = ByteBuffer.Allocate(24);
            header.Order(ByteOrder.BigEndian);
            // so baaad
            header.PutInt(flag);
            header.PutInt(messageBuffer.Remaining());
            MD5 hash = MD5.Create()  ;
            byte[] hashvalue = hash.ComputeHash(message,messageBuffer.ArrayOffset(),messageBuffer.Capacity());
            for (int i = 0; i < 16; i++)
                header.Put((sbyte)hashvalue[15 - i]);
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
            messageBuffer.Rewind();
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
            }
            return returned;
        }

        KeyValuePair<int,byte[]> ReciveFrame()
        {
            Selector selector = Selector.Open();
            SelectionKey pipeKey = model.interruptPipeSource.Register(selector, Operations.Read); ;
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
            ByteBuffer messageFrameBuf = ByteBuffer.Wrap(messageFrame);

            while (messageFrameBuf.HasRemaining) //recive message
            {
                selector.Select();
                ICollection<SelectionKey> selectedKeys = selector.SelectedKeys();
                foreach (SelectionKey key in selectedKeys)
                    if (key == pipeKey)
                        throw new System.Exception("pipe");
                socket.Read(messageFrameBuf);
            }

            MD5 md5 = MD5.Create();
            byte[] targetHash=md5.ComputeHash(messageFrame);
            for (int i = 0; i < 6; i++)
                if (targetHash[i] != hash[i])
                    throw new System.Exception("checksum");

            return new KeyValuePair<int, byte[]>(flag, messageFrame);
        }

    }

}