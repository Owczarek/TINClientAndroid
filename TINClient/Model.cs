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

namespace TINClient
{
    class Model
    {
        public Pipe interruptPipe;
        public InetSocketAddress serwerAddress;
    }
}