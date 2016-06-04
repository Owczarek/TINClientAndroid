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
using Java.Nio.Channels;
using System.Threading;
using Java.Net;
using Java.Nio;
namespace TINClient
{
    [Activity(Label = "TINClient", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {

        Button connectButton;
        Button sendButton;
        Button reciveButton;
        Button disconnectButton;
        EditText addressText;
        EditText portText;
        TextView outputText;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);


            StrictMode.ThreadPolicy policy = new StrictMode.ThreadPolicy.Builder().PermitAll().Build();
            StrictMode.SetThreadPolicy(policy);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
             connectButton = FindViewById<Button>(Resource.Id.Connect);
             sendButton = FindViewById<Button>(Resource.Id.Send);
             reciveButton = FindViewById<Button>(Resource.Id.Recive);
             disconnectButton = FindViewById<Button>(Resource.Id.Disconnect);
             addressText = FindViewById<EditText>(Resource.Id.Address);
             portText = FindViewById<EditText>(Resource.Id.Port);
             outputText = FindViewById<TextView>(Resource.Id.Output);

            

            Model Model;
          //  Model.mainActivity = this; later

            this.StartService(new Intent(this, typeof(ConnectionService)));

            ConnectionServiceConnection serviceConnection = new ConnectionServiceConnection(null);

            // bind to service
            Intent locationServiceIntent =
                new Intent(Android.App.Application.Context, typeof(ConnectionService));
            bool a=Android.App.Application.Context.BindService(
                locationServiceIntent, serviceConnection, Bind.AutoCreate);


            



            connectButton.Click += delegate
            {
                if (Model.logicLayer == null)
                {


                    Model.serwerAddress = new InetSocketAddress(InetAddress.GetByName(addressText.Text), Int32.Parse(portText.Text));

                    Byte[] signal = new byte[1];
                    signal[0] = (byte)Signal.Connect;
                    ByteBuffer sigBuf = ByteBuffer.Wrap(signal);
                    int sent = Model.communicationPipeSink.Write(sigBuf);


                }

                // connectionThread.Join();
            };

            disconnectButton.Click += delegate
            {
                if (Model.logicLayer != null)
                {
                    Byte[] signal = new byte[1];
                    signal[0] = 0;
                    int sent=Model.interruptPipe.Sink().Write(ByteBuffer.Wrap(signal));

                    
                }

            };

            sendButton.Click += delegate
            {
                if (Model.logicLayer != null)
                {
                    Byte[] signal = new byte[1];
                    signal[0] = (byte)Signal.Send;
                    ByteBuffer sigBuf = ByteBuffer.Wrap(signal);
                    int sent=Model.communicationPipeSink.Write(sigBuf);
                 //   int sent=Model.communicationPipe.Sink().Write(ByteBuffer.Wrap(signal));
                }

            };
            reciveButton.Click += delegate
            {
                if (Model.logicLayer != null)
                {
                    Byte[] signal = new byte[1];
                    signal[0] = (byte)Signal.Recive;
                    ByteBuffer sigBuf = ByteBuffer.Wrap(signal);
                    int sent = Model.communicationPipeSink.Write(sigBuf);
                    //   int sent=Model.communicationPipe.Sink().Write(ByteBuffer.Wrap(signal));
                }

            };


            

        }
        public void Output(string a)
        {
            RunOnUiThread(() => outputText.Text = a);
        }
    }
}

