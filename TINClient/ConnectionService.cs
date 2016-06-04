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



            Model.interruptPipe = Pipe.Open();
            Model.communicationPipe = Pipe.Open();
            Model.communicationPipeSink = Model.communicationPipe.Sink();
            Model.interruptPipeSink = Model.interruptPipe.Sink();
            Model.communicationPipeSource = Model.communicationPipe.Source();
            Model.interruptPipeSource = Model.interruptPipe.Source();
            Model.interruptPipeSource.ConfigureBlocking(false);
            Model.communicationPipeSource.ConfigureBlocking(false);




      //      Model.serwerAddress = new InetSocketAddress(InetAddress.GetByName(addressText.Text), Int32.Parse(portText.Text));



            Model.logicLayer = new LogicLayer();


            Model.connectionThread = new Thread(Model.logicLayer.Run);

            Model.connectionThread.Start();


        }
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
          /*  // start a task here
            new Task(() => {
                // long running code
                DoWork();
            }).Start();*/
            return StartCommandResult.Sticky;
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


}
