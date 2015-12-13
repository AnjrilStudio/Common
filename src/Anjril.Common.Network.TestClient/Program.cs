using Anjril.Common.Network.UdpImpl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Anjril.Common.Network.TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var listeningPort = 15000;
            var remotePort = 16000;

            var udpClient = new UdpClient(listeningPort);

            var socket = new Socket(new UdpReceiver(udpClient), new UdpSender(udpClient), MessageReceived);

            socket.StartListening();

            var endPoint = new RemoteConnection
            {
                IPAddress = "127.0.0.1",
                Port = remotePort
            };

            while(true)
            {
                Console.Write("Veuillez saisir un message à envoyer : ");
                var message = Console.ReadLine();

                socket.Send(message, endPoint);
            }
        }

        private static void MessageReceived(object sender, string message)
        {
            Console.WriteLine("Message reçu : " + message);
        }
    }
}
