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

            //button.Click += delegate { button.Text = string.Format("{0} clicks!", count++); };

            Model model=new Model();
            model.mainActivity = this;


            connectButton.Click += delegate
            {
                if (model.logicLayer == null)
                {

                    model.interruptPipe = Pipe.Open();
                    model.communicationPipe = Pipe.Open();
                    model.communicationPipeSink = model.communicationPipe.Sink();
                    model.interruptPipeSink = model.interruptPipe.Sink();
                    model.communicationPipeSource = model.communicationPipe.Source();
                    model.interruptPipeSource = model.interruptPipe.Source();
                    model.interruptPipeSource.ConfigureBlocking(false);
                    model.communicationPipeSource.ConfigureBlocking(false);




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
                    Byte[] signal = new byte[1];
                    signal[0] = 0;
                    int sent=model.interruptPipe.Sink().Write(ByteBuffer.Wrap(signal));

                    model.connectionThread.Join();
                    model.connectionThread = null;

                    model.logicLayer = null;
                    model.communicationPipe.Dispose();
                    model.communicationPipe = null;
                    model.interruptPipe.Dispose();
                    model.interruptPipe = null;
                }

            };

            sendButton.Click += delegate
            {
                if (model.logicLayer != null)
                {
                    Byte[] signal = new byte[1];
                    signal[0] = (byte)Signal.Send;
                    ByteBuffer sigBuf = ByteBuffer.Wrap(signal);
                    int sent=model.communicationPipeSink.Write(sigBuf);
                 //   int sent=model.communicationPipe.Sink().Write(ByteBuffer.Wrap(signal));
                }

            };
            reciveButton.Click += delegate
            {
                if (model.logicLayer != null)
                {
                    Byte[] signal = new byte[1];
                    signal[0] = (byte)Signal.Recive;
                    ByteBuffer sigBuf = ByteBuffer.Wrap(signal);
                    int sent = model.communicationPipeSink.Write(sigBuf);
                    //   int sent=model.communicationPipe.Sink().Write(ByteBuffer.Wrap(signal));
                }

            };


            

        }
        public void Output(string a)
        {
            RunOnUiThread(() => outputText.Text = a);
        }
    }
}

