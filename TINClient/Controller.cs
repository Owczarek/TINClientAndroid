using Java.Nio.Channels;
using Java.Net;
using Java.Nio;
using System.Collections.Generic;
using System.Security.Cryptography;
using System;namespace TINClient
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
                            if (signal.Get() == (byte)Signal.Send)
                            {
                                byte[] data = new byte[20];
                                for (byte i = 0; i < 20; i++)
                                {
                                    data[i] = (byte)(0x61 + i);
                                }
                                securityLayer.Send(data);
                            }
                        }
                    }
                    
                }
            }
            catch
            {
                
                Disconnect();
            }
        }

        void Disconnect()
        {
            model.interruptPipeSource.Read(ByteBuffer.Allocate(10));
            model.communicationPipeSource.Read(ByteBuffer.Allocate(10));
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

        public ByteBuffer Recive()
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
        SelectionKey pipeKey;
        public TCPLayer(Model m)
        {
            model = m;
            Selector selector=Selector.Open();
            SelectionKey socketKey;
            socket = SocketChannel.Open();
            socket.ConfigureBlocking(false);
            socketKey = socket.Register(selector, Operations.Connect);
            Pipe.SourceChannel pipeSource = model.interruptPipe.Source();
            pipeSource.ConfigureBlocking(false);

            pipeKey = pipeSource.Register(selector,Operations.Read);
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
            }

            SendFrame(message,ByteBuffer.Wrap(message),1);
        }

        void SendFrame(byte[] message,ByteBuffer messageBuffer,int flag)
        {
            Selector selector = Selector.Open();
            SelectionKey socketKey = socket.Register(selector, Operations.Write);
            pipeKey = model.interruptPipe.Source().Register(selector, Operations.Read);

            ByteBuffer header = ByteBuffer.Allocate(24);
            header.Order(ByteOrder.BigEndian);
            // so baaad
            header.PutInt(flag);
            int a = messageBuffer.ArrayOffset();
            int b = messageBuffer.Position();
            int c = messageBuffer.Limit();
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


        public ByteBuffer Recive()
        {
            return ByteBuffer.Allocate(1);
        }
    }

}