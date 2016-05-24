using Anjril.Common.Network.Exceptions;
using Anjril.Common.Network.TcpImpl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anjril.Common.Network.Client
{
    class Program
    {
        private const int SERVER_PORT = 15000;
        private const string SEP = "<sep>";

        private static ISocketClient CLIENT;

        static void Main(string[] args)
        {
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("---------- CLIENT SAMPLE ----------");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine();

            var rand = new Random();
            var port = rand.Next(1500, 65000);

            using (CLIENT = new TcpSocketClient(port, SEP))
            {
                Console.Write("Please, type your username: ");
                var username = Console.ReadLine();
                Console.WriteLine();

                Console.WriteLine("Connecting...");
                try
                {
                    var greetings = CLIENT.Connect("127.0.0.1", SERVER_PORT, OnMessageReceived, username);
                    Console.WriteLine("Client connected on port: " + port);
                    Console.WriteLine();

                    bool quit = false;
                    while (!quit)
                    {
                        Console.WriteLine("Press 'q' to stop the client, 'm' to type a new message...");
                        Console.WriteLine();

                        Console.WriteLine(greetings);
                        Console.WriteLine();

                        var key = Console.ReadKey(true);

                        if (key.Key == ConsoleKey.M)
                        {
                            GetMessage();
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

                    Console.Write("Any last words? ");
                    var lastWords = Console.ReadLine();
                    CLIENT.Disconnect(lastWords);
                    Console.WriteLine();

                    Console.WriteLine("The client is disconnected!");
                    Console.WriteLine();
                }
                catch (ConnectionFailedException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine();
                }
            }

            Console.WriteLine("-----------------------------------");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("---------- CLIENT SAMPLE ----------");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine();

            Console.WriteLine("Press any key to finish...");
            Console.ReadKey(true);
        }

        private static void GetMessage()
        {
            Console.Write("Enter your message: ");
            var lastWords = Console.ReadLine();
            Console.WriteLine();

            CLIENT.Send(lastWords);

            Console.WriteLine("Message sent!");
            Console.WriteLine();
        }

        private static void OnMessageReceived(IRemoteConnection sender, string message)
        {
            Console.WriteLine(message);
            Console.WriteLine();
        }
    }
}
