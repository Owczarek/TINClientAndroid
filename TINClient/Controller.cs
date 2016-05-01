using Java.Nio.Channels;
using Java.Net;
using Java.Nio;
using System.Collections.Generic;
using System.Security.Cryptography;
using System;
namespace TINClient
{
    internal class Controller
    {

    }


    internal class LogicLayer
    {
        public LogicLayer(Model m)
        {
            model = m;
            securityLayer = new SecurityLayer(model);
        }
        public void Run()
        {
            securityLayer.Send(new byte[2]);
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


    internal class TCPLayer
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
            pipeKey = model.interruptPipe.Source().Register(selector,Operations.Read);
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
            SendFrame(message,1);
        }

        void SendFrame(byte[] message,int flag)
        {
            Selector selector = Selector.Open();
            SelectionKey socketKey = socket.Register(selector, Operations.Write);
            pipeKey = model.interruptPipe.Source().Register(selector, Operations.Read);

            byte[] header= new byte[24];

            // so baaad
            header[3] = (byte)message.Length;
            header[2] = (byte)(message.Length>>8);
            header[1] = (byte)(message.Length >> 16);
            header[0] = (byte)(message.Length >> 24);

            header[4] = 0;
            header[5] = 0;
            header[6] = 0;
            header[7] = (byte)flag;

            MD5.Create().TransformBlock(message,0,message.Length,header,8);


            int leftBytes = 24;
            while (leftBytes>0) //send header
            {
                selector.Select();
                ICollection<SelectionKey> selectedKeys = selector.SelectedKeys();
                foreach (SelectionKey key in selectedKeys)
                    if(key == pipeKey)
                        throw new System.Exception("pipe");
                leftBytes-=socket.Write(ByteBuffer.Wrap(header, 24 - leftBytes, leftBytes));
            }

            leftBytes = message.Length;
            while (leftBytes > 0) //send payload
            {
                selector.Select();
                ICollection<SelectionKey> selectedKeys = selector.SelectedKeys();
                foreach (SelectionKey key in selectedKeys)
                    if (key == pipeKey)
                        throw new System.Exception("pipe");
                leftBytes -= socket.Write(ByteBuffer.Wrap(header, message.Length - leftBytes, leftBytes));
            }
        }


        public ByteBuffer Recive()
        {
            return ByteBuffer.Allocate(1);
        }
    }

}