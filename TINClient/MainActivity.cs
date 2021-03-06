﻿using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
//using Java.Lang;
//using System.Net.Sockets;
using System.IO;
using Java.Nio.Channels;
using System.Threading;
using Java.Net;
using Java.Nio;
using System.Security.Cryptography;

namespace TINClient
{
    [Activity(Label = "TINClient", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {

        Button connectButton;
        Button disconnectButton;
        Button settingsButton;
        Button filesButton;
        TextView outputText;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
             connectButton = FindViewById<Button>(Resource.Id.Connect);
             
             disconnectButton = FindViewById<Button>(Resource.Id.Disconnect);

            settingsButton = FindViewById<Button>(Resource.Id.Settings);

            filesButton = FindViewById<Button>(Resource.Id.Files);

            outputText = FindViewById<TextView>(Resource.Id.Output);


            this.StartService(new Intent(this, typeof(ConnectionService)));

            //ConnectionServiceConnection serviceConnection = new ConnectionServiceConnection(null);

            // bind to service
          //  Intent connectionServiceIntent =
           //     new Intent(Android.App.Application.Context, typeof(ConnectionService));
           // bool a=Android.App.Application.Context.BindService(
           //     connectionServiceIntent, serviceConnection, Bind.AutoCreate);


            



            connectButton.Click += delegate
            {


                    //  Model.instance.serwerAddress = new InetSocketAddress(InetAddress.GetByName(addressText.Text), Int32.Parse(portText.Text));
                    /*
                    Byte[] signal = new byte[1];
                    signal[0] = (byte)Signal.Connect;
                    ByteBuffer sigBuf = ByteBuffer.Wrap(signal);
                    int sent = Model.instance.communicationPipeSink.Write(sigBuf);*/
                    //   Model.instance.connectionThread = new Thread(Model.instance.logicLayer.Run);
                    //   Model.instance.connectionThread.Start();
                
                if (Model.instance.connectionThread==null)
                {
                    Model.instance.connectionThread = new Thread(Model.instance.logicLayer.Run);
                    Model.instance.connectionThread.Start();
                }
                

                // connectionThread.Join();
            };

            disconnectButton.Click += delegate
            {
                if (Model.instance.connectionThread != null)
                {
                    Byte[] signal = new byte[1];
                    signal[0] = 0;
                    int sent=Model.instance.interruptPipe.Sink().Write(ByteBuffer.Wrap(signal));                
                }

            };


            settingsButton.Click += delegate
            {
                var intent = new Intent(this, typeof(Settings));
                StartActivity(intent);
            };

            filesButton.Click += delegate
            {
                var intent = new Intent(this, typeof(Files));
                StartActivity(intent);
            };



        }
        public void Output(string a)
        {
            RunOnUiThread(() => outputText.Text = a);
        }

        protected override void OnPause()
        {
            Model.instance.mainActivity = null;
            base.OnPause();
        }
        protected override void OnResume()
        {
            Model.instance.mainActivity = this;
            base.OnResume();
        }

        protected override void OnStop()
        {
            Model.Serialize(Model.savePath);
            base.OnDestroy();
        }
    }
}

