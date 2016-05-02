﻿using Java.Nio.Channels;
using Java.Net;
using Java.Nio;
using System.Collections.Generic;
using System.Security.Cryptography;
using System;namespace TINClient
{

    internal class LogicLayer
    {
        public LogicLayer(Model m)
        {
            model = m;
            
        }
        public void Run()
        {
            try
            {
                securityLayer = new SecurityLayer(model);
                byte[] data = new byte[20];
                for(byte i=0;i<20;i++)
                {
                    data[i] =(byte) (0x61 + i);
                }
                securityLayer.Send(data);
            }
            catch (Exception e)
            {

            }
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
            SendFrame(message,1);
        }

        void SendFrame(byte[] message,int flag)
        {
            Selector selector = Selector.Open();
            SelectionKey socketKey = socket.Register(selector, Operations.Write);
            pipeKey = model.interruptPipe.Source().Register(selector, Operations.Read);

            byte[] header= new byte[24];

            // so baaad
            header[7] = (byte)message.Length;
            header[6] = (byte)(message.Length>>8);
            header[5] = (byte)(message.Length >> 16);
            header[4] = (byte)(message.Length >> 24);

            header[0] = 0;
            header[1] = 0;
            header[2] = 0;
            header[3] = (byte)flag;

            MD5 hash = MD5.Create()
;
            byte[] hashvalue = hash.ComputeHash(message);
            for(int i=0;i<16;i++)
                header[23-i]=hashvalue[i];

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
                leftBytes -= socket.Write(ByteBuffer.Wrap(message, message.Length - leftBytes, leftBytes));
            }
        }


        public ByteBuffer Recive()
        {
            return ByteBuffer.Allocate(1);
        }
    }

}