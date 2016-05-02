using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
//using Java.Lang;
//using System.Net.Sockets;
using System.IO;
using Mono.Unix.Native;
using Java.Nio.Channels;
using System.Threading;
using Java.Net;
using Java.Nio;
namespace TINClient
{
    [Activity(Label = "TINClient", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        int count = 1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button connectButton = FindViewById<Button>(Resource.Id.Connect);
            Button sendButton = FindViewById<Button>(Resource.Id.Send);
            Button reciveButton = FindViewById<Button>(Resource.Id.Recive);
            Button disconnectButton = FindViewById<Button>(Resource.Id.Disconnect);
            EditText addressText = FindViewById<EditText>(Resource.Id.Address);
            EditText portText = FindViewById<EditText>(Resource.Id.Port);

            //button.Click += delegate { button.Text = string.Format("{0} clicks!", count++); };



            Model model = new Model();
            byte[] address = new byte[4];
            address[0] = 192;
            address[1] = 168;
            address[2] = 0;
            address[3] = 15;





            model.interruptPipe = Pipe.Open();
            model.communicationPipe = Pipe.Open();

            connectButton.Click += delegate
            {
                if (model.logicLayer == null)
                {




                    model.serwerAddress = new InetSocketAddress(InetAddress.GetByName(addressText.Text), Int32.Parse(portText.Text));



                    model.logicLayer = new LogicLayer(model);


                    model.connectionThread = new Thread(model.logicLayer.Run);

                    model.connectionThread.Start();
                }

                // connectionThread.Join();
            };

            disconnectButton.Click += delegate
            {
                if (model.logicLayer != null)
                {
                    ByteBuffer signal = ByteBuffer.Allocate(1);
                    model.interruptPipe.Sink().Write(signal);
                    model.connectionThread.Join();

                    model.logicLayer = null;
                }

            };

            sendButton.Click += delegate
            {
                if (model.logicLayer != null)
                {
                    ByteBuffer signal = ByteBuffer.Allocate(1);
                    signal.Put((sbyte)(byte)Signal.Send);
                    model.communicationPipe.Sink().Write(signal);
                }

            };


        }
    }
}

