using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Nio;
using Java.Nio.Channels;
using Java.Net;
using System.Threading;

namespace TINClient
{
    [Service]
    public class ConnectionService : Service
    {
        public override void OnCreate()
        {
            base.OnCreate();

            StrictMode.ThreadPolicy policy = new StrictMode.ThreadPolicy.Builder().PermitAll().Build();
            StrictMode.SetThreadPolicy(policy);

            Model.instance.interruptPipe = Pipe.Open();
            Model.instance.interruptPipeSource = Model.instance.interruptPipe.Source();
            Model.instance.interruptPipeSink = Model.instance.interruptPipe.Sink();
            Model.instance.interruptPipeSource.ConfigureBlocking(false);
            Model.instance.communicationPipe = Pipe.Open();
            Model.instance.communicationPipeSink = Model.instance.communicationPipe.Sink();
            
            Model.instance.communicationPipeSource = Model.instance.communicationPipe.Source();
            
            Model.instance.communicationPipeSource.ConfigureBlocking(false);




      //      Model.instance.serwerAddress = new InetSocketAddress(InetAddress.GetByName(addressText.Text), Int32.Parse(portText.Text));



            


            if(Model.instance.autoconnect)
            {
                
            }
            


        }
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            /*  // start a task here
              new Task(() => {
                  // long running code
                  DoWork();
              }).Start();*/
          //  if (Model.instance.connectionThread == null)
          //  { 
          //      Model.instance.connectionThread = new Thread(Model.instance.logicLayer.Run);
          //      Model.instance.connectionThread.Start();
          //  }
            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            Model.Serialize(Model.savePath);
            base.OnDestroy();
        }


        public override IBinder OnBind(Intent intent)
        {
            return new ConnectionServiceBinder(this);
        }
    }

    public class ConnectionServiceBinder : Binder
    {
        public ConnectionService Service
        {
            get { return this.service; }
            
        } protected ConnectionService service;

        public ConnectionServiceBinder(ConnectionService service) { this.service = service; }
    }







    public class ConnectionServiceConnection : Java.Lang.Object, IServiceConnection
    {
        public ConnectionServiceBinder binder;
        public ConnectionServiceConnection (ConnectionServiceBinder binder)
        {
            if (binder != null)
            {
                this.binder = binder;
            }
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            ConnectionServiceBinder serviceBinder = service as
            ConnectionServiceBinder;

            if (serviceBinder != null)
            {
                this.binder = serviceBinder;
             //   this.binder.IsBound = true;

           //     // raise the service bound event
          //      this.ServiceConnected(this, new
          //      ServiceConnectedEventArgs()
          //      { Binder = service });

                // begin updating the location in the Service
          //      serviceBinder.Service.StartLocationUpdates();
            }
        }

        public void OnServiceDisconnected(ComponentName name) { /*this.binder.IsBound = false;*/ }
    }






    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { Intent.ActionBootCompleted })]
    class BootCompletedBroadcastMessageReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action == Intent.ActionBootCompleted)
            {
                Application.Context.StartService(new Intent(Application.Context, typeof(ConnectionService)));
            }
        }
    }


}
