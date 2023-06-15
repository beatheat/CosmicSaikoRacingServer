using EdenNetwork;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using EdenNetwork.Log;

namespace MatchingServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "CosmicSaikoRacing - MatchingServer";
            EdenLogManager.SettingLogger("EdenNetwork", "log", printConsole: true);
            EdenUdpServer server = new EdenUdpServer(16969);
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
