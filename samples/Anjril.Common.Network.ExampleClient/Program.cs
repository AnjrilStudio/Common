using Anjril.Common.Network;
using Anjril.Common.Network.UdpImpl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Anjril.Common.Network.ExampleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var listeningPort = 15000;
            var remotePort = 16000;

            var socket = new UdpSocket(listeningPort, MessageReceived, MessageReceived, MessageReceived, MessageReceived);

            socket.StartListening();

            var endPoint = new UdpRemoteConnection("127.0.0.1", remotePort);

            while(true)
            {
                Console.Write("Veuillez saisir un message à envoyer : ");

                var message = Console.ReadLine();

                socket.Send(message, endPoint);
            }
        }

        private static void MessageReceived(IRemoteConnection sender, string message)
        {
            Console.WriteLine("Message reçu : " + message);
        }
    }
}
