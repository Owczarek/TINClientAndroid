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
using Java.Nio.Channels;
using Java.Net;
using System.Threading;

namespace TINClient
{
    public class Model
    {
        static public Pipe interruptPipe;
        static public Pipe communicationPipe;
        static public Pipe.SinkChannel communicationPipeSink;
        static public Pipe.SinkChannel interruptPipeSink;
        static public Pipe.SourceChannel interruptPipeSource;
        static public Pipe.SourceChannel communicationPipeSource;
        static public InetSocketAddress serwerAddress;
        static public LogicLayer logicLayer;
        static public Thread connectionThread;
        static public int FrameSize=30;
        static public MainActivity mainActivity;

        static public byte[] username= {(byte)'a',(byte)'b' };//should be initialized from conf file
        static public byte[] password= {(byte)'a',(byte)'b', (byte)'c' };


    static public void DestroyConnection()
        {
            logicLayer = null;
            communicationPipe.Dispose();
            communicationPipe = null;
            interruptPipe.Dispose();
            interruptPipe = null;
          //  mainActivity.RunOnUiThread(() => { connectionThread.Join(); connectionThread = null; });
          //  mainActivity.Output("disconnected");
        }
    }



    public enum Signal : byte
    {
        Send = 0,
        Recive =1,
        Connect=2
    }

}