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
    class Model
    {
        public Pipe interruptPipe;
        public Pipe communicationPipe;
        public Pipe.SinkChannel communicationPipeSink;
        public Pipe.SinkChannel interruptPipeSink;
        public InetSocketAddress serwerAddress;
        public LogicLayer logicLayer;
        public Thread connectionThread;
        public int FrameSize=5;
    }


    public enum Signal : byte
    {
        Send = 0
    }

}