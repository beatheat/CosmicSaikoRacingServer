﻿using EdenNetwork;
using System.Text.Json;

namespace MatchingServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "CosmicSaikoRacing - MatchingServer";
            EdenNetServer server = new EdenNetServer(16969, "MatchingServerLog.txt");
            MatchingServer matchingServer = new MatchingServer(server);
            matchingServer.Run();

            Console.WriteLine("Ctrl+c to close server");
            Console.CancelKeyPress += (object? sender, ConsoleCancelEventArgs eventArgs) =>
            {
                matchingServer.Close();
            };
            while (true)
            {
                Thread.Sleep(100);
            }
        }
    }
}
