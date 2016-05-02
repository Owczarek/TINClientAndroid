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
            Button button = FindViewById<Button>(Resource.Id.MyButton);

            //button.Click += delegate { button.Text = string.Format("{0} clicks!", count++); };
            Model model = new Model();
            byte[] address=new byte[4];
            address[0] = 192;
            address[1] = 168;
            address[2] = 0;
            address[3] = 15;



            model.serwerAddress = new InetSocketAddress(InetAddress.GetByAddress(address),1266);

            model.interruptPipe = Pipe.Open();


            button.Click += delegate 
            {

                LogicLayer logicLayer = new LogicLayer(model);

                Thread connectionThread = new Thread(logicLayer.Run);

                connectionThread.Start();
                connectionThread.Join();
            };
        }
    }
}

