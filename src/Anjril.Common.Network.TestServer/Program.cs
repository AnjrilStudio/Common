using Anjril.Common.Network.UdpImpl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Anjril.Common.Network.TestServer
{
    class Program
    {
        private static Socket Socket;

        static void Main(string[] args)
        {
            var listeningPort = 16000;

            var udpClient = new UdpClient(listeningPort);

            Socket = new Socket(new UdpReceiver(udpClient), new UdpSender(udpClient), MessageReceived);

            Socket.StartListening();
            Console.WriteLine("Le serveur commence à écouter.");

            Console.WriteLine();
            Console.WriteLine("Appuyez sur n'importe quelle touche pour quitter.");
            Console.ReadKey();
        }

        public static void MessageReceived(RemoteConnection sender, string message)
        {
            Console.WriteLine("Message reçu : " + message);

            Socket.Send("Message bien reçu !", sender);
        }
    }
}
