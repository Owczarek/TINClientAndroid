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
using System.Xml.Serialization;
using System.Xml;
using System.IO;

namespace TINClient
{
    public sealed class Model
    {
        static Model Instance = null;
        private static readonly object _lock = new object();
        [XmlIgnore]
        public static Model instance
        {
            get
            {
                lock(_lock)
                {
                    if (Instance == null)
                    {
                        // Construct an instance of the XmlSerializer with the type
                        // of object that is being deserialized.
                        FileStream myFileStream=null;
                        try
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(Model));
                            // To read the file, create a FileStream.
                            myFileStream = new FileStream(savePath, FileMode.Open);
                            // Call the Deserialize method and cast to the object type.
                            Instance = (Model)serializer.Deserialize(myFileStream);
                            myFileStream.Close();
                        }
                        catch (System.Exception e)
                        {
                            string exception = e.Message;
                            Instance = new Model();
                        }
                        finally
                        {
                            if(myFileStream!=null)
                            {
                                myFileStream.Close();
                            }
                        }

                        Instance.logicLayer = new LogicLayer();
                        Instance.serwerAddress = new InetSocketAddress(InetAddress.GetByName(Encoding.ASCII.GetString(Instance.address)), Instance.port);

                    }
                    return Instance;
                }
                
            }
        }
        Model()
        {
            
            // Initialize.
        }

        public static string savePath ="/sdcard/TIN/conf.txt";
        [XmlIgnore]
        public Pipe interruptPipe;
        [XmlIgnore]
        public Pipe communicationPipe;
        [XmlIgnore]
        public Pipe.SinkChannel communicationPipeSink;
        [XmlIgnore]
        public Pipe.SinkChannel interruptPipeSink;
        [XmlIgnore]
        public Pipe.SourceChannel interruptPipeSource;
        [XmlIgnore]
        public Pipe.SourceChannel communicationPipeSource;
        [XmlIgnore]
        public InetSocketAddress serwerAddress;
        [XmlIgnore]
        public LogicLayer logicLayer;
        [XmlIgnore]
        public Thread connectionThread;
        public int FrameSize=50;
        public int LogicFrameSize = 1000;
        [XmlIgnore]
        public MainActivity mainActivity;


        public int port = 1;
        public byte[] address = { (byte)'0', (byte)'.', (byte)'0', (byte)'.', (byte)'0', (byte)'.', (byte)'0' };
        public byte[] username= {(byte)'a',(byte)'b' };//should be initialized from conf file
        public byte[] password= {(byte)'a',(byte)'b', (byte)'c' };//without /0
        public List<string> files;
        public byte[]  machinename = { (byte)'x', (byte)'y', (byte)'z' };//without /0
        public DateTime timeLastSynchronized = DateTime.Now.AddHours(-1);
        public bool autoconnect=true;
        [XmlIgnore]
        public State connectionState= State.Disconnected;



        public void DestroyConnection()
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




        public static void Serialize(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Model));
            using (TextWriter writer = new StreamWriter(filename))
            {
                serializer.Serialize(writer, Model.Instance);
                writer.Close();
            }
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