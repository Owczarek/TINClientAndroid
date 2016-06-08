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
    static public class Model
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
        static public int LogicFrameSize = 300;
        static public MainActivity mainActivity;

        static public byte[] username= {(byte)'a',(byte)'b' };//should be initialized from conf file
        static public byte[] password= {(byte)'a',(byte)'b', (byte)'c' };//without /0
        static public List<string> files;
        static public byte[]  machinename = { (byte)'x', (byte)'y', (byte)'z' };//without /0
        static public DateTime timeLastSynchronized = DateTime.Now.AddHours(-1);
        static public bool autoconnect=true;

        static public State connectionState= State.Disconnected;


        static Model()
        {
            files = new List<string>();
            files.Add("/sdcard/test/abc.txt");
        }



        static public void DestroyConnection()
        {
           // logicLayer = null;
          //  communicationPipe.Dispose();
         //   communicationPipe = null;
         //   interruptPipe.Dispose();
           // interruptPipe = null;
         //  if(mainActivity!=null)
        //    mainActivity.RunOnUiThread(() => { connectionThread.Join(); connectionThread = null; });
          //  mainActivity.Output("disconnected");
        }
    }

    public enum State : byte
    {
        Disconnected,
        Logged,
        Sending
    }

    public enum Signal : byte
    {
        Send = 0,
        Recive =1,
        Connect=2
    }

}