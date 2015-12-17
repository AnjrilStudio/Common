using Anjril.Common.Network.UdpImpl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Anjril.Common.Network.ExampleServer
{
    class Program
    {
        private static ISocket Socket;

        static void Main(string[] args)
        {
            var listeningPort = 16000;

            Socket = new UdpSocket(listeningPort, ConnectionRequested, ConnectionAccepted, ConnectionRefused, MessageReceived);

            Socket.StartListening();

            Console.WriteLine("Le serveur commence à écouter.");

            Console.WriteLine();
            Console.WriteLine("Appuyez sur n'importe quelle touche pour quitter.");
            Console.ReadKey();
        }

        public static void ConnectionRequested(IRemoteConnection sender, string message)
        {
            throw new NotImplementedException();
        }

        public static void ConnectionAccepted(IRemoteConnection sender, string message)
        {
            throw new NotImplementedException();
        }

        public static void ConnectionRefused(IRemoteConnection sender, string message)
        {
            throw new NotImplementedException();
        }

        public static void MessageReceived(IRemoteConnection sender, string message)
        {
            Console.WriteLine("Message reçu : " + message);
        }
    }
}
