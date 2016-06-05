using Anjril.Common.Network.TcpImpl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anjril.Common.Network.Sample.Server
{
    class Program
    {
        private const int PORT = 15000;
        private const string SEP = "<sep>";

        private static IDictionary<IRemoteConnection, string> USERNAMES;
        private static ISocket SOCKET;

        static void Main(string[] args)
        {
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("---------- SERVER SAMPLE ----------");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine();

            USERNAMES = new Dictionary<IRemoteConnection, string>();

            using (SOCKET = new TcpSocket(PORT, SEP))
            {
                Console.WriteLine("Connecting...");
                SOCKET.StartListening(OnConnectionRequested, OnMessageReceived, OnDisconnect);
                Console.WriteLine("Server connected on port: " + PORT);
                Console.WriteLine();

                bool quit = false;
                while (!quit)
                {
                    Console.WriteLine("Press 'q' to stop the server, 'l' to print the remote connected...");
                    Console.WriteLine();

                    var key = Console.ReadKey(true);

                    if (key.Key == ConsoleKey.L)
                    {
                        ShowRemoteConnected();
                    }
                    else if (key.Key == ConsoleKey.Q)
                    {
                        quit = true;
                    }
                    else
                    {
                        Console.Write("Please, use a valid command...");
                    }
                }

                SOCKET.StopListening();

                Console.WriteLine("The server is stopped!");
                Console.WriteLine();
            }

            Console.WriteLine("-----------------------------------");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("---------- SERVER SAMPLE ----------");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine();

            Console.WriteLine("Press any key to finish...");
            Console.ReadKey(true);
        }

        private static void ShowRemoteConnected()
        {
            Console.WriteLine("The list of remote connected:");

            foreach (var pair in USERNAMES)
            {
                Console.WriteLine("- {0} from {1}:{2}", pair.Value, pair.Key.IPAddress, pair.Key.Port);
            }

            if (USERNAMES.Count == 0)
            {
                Console.WriteLine("- No remote connected");
            }
            Console.WriteLine();
        }

        private static void OnDisconnect(IRemoteConnection remote, string justification)
        {
            var username = USERNAMES[remote];

            Console.WriteLine("{0} ({1}:{2}) is disconnected ({3})", username, remote.IPAddress, remote.Port, justification);
            Console.WriteLine();

            SOCKET.Broadcast(String.Format("{0} has disconnected ({1})", username, justification));

            USERNAMES.Remove(remote);

            Console.WriteLine("Press 'q' to stop the server, 'l' to print the remote connected...");
            Console.WriteLine();
        }

        private static void OnMessageReceived(IRemoteConnection sender, string message)
        {
            var username = USERNAMES[sender];

            Console.WriteLine("New message received from {0}: {1}", username, message);
            Console.WriteLine();

            SOCKET.Broadcast(String.Format("Message sent by {0}: {1}", username, message));

            Console.WriteLine("Press 'q' to stop the server, 'l' to print the remote connected...");
            Console.WriteLine();
        }

        private static bool OnConnectionRequested(IRemoteConnection sender, string request, out string response)
        {
            bool connectionGranted = !USERNAMES.Values.Contains(request);

            if (!connectionGranted)
            {
                response = "An other remote is already connected with the same username.";

                Console.WriteLine("{0}:{1} tries to connect with the currently used username: {2}", sender.IPAddress, sender.Port, request);
                Console.WriteLine();
            }
            else
            {
                response = "Welcome " + request;
                USERNAMES.Add(sender, request);

                Console.WriteLine("{0}:{1} connects with the current username: {2}", sender.IPAddress, sender.Port, request);
                Console.WriteLine();
            }

            Console.WriteLine("Press 'q' to stop the server, 'l' to print the remote connected...");
            Console.WriteLine();

            return connectionGranted;
        }
    }
}
