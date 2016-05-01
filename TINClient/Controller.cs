using Java.Nio.Channels;


namespace TINClient
{
    internal class Controller
    {

    }


    internal class LogicLayer
    {
        public LogicLayer(Model m)
        {
            model = m;
            securityLayer = new SecurityLayer(model);
        }
        Model model;
        SecurityLayer securityLayer;
    }


    internal class SecurityLayer
    {
        Model model;
        TCPLayer tcpLayer;
        public SecurityLayer(Model m)
        {
            model = m;
            tcpLayer = new TCPLayer(model);
        }
        static void send()
        {

        }

        static void recive()
        {

        }

    }


    internal class TCPLayer
    {
        Model model;
        SocketChannel socket;
        Selector selector;
        public TCPLayer(Model m)
        {
            model = m;

        }
        static void send()
        {

        }

        static void recive()
        {

        }
    }

}