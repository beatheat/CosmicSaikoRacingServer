using EdenNetwork;
using System.Text.Json;

namespace MatchingServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "CosmicSaikoRacing - MatchingServer";
            EdenNetServer server = new EdenNetServer(16969, "test.txt");
            MatchingServer matchingServer = new MatchingServer(server);
            matchingServer.Run();

            Console.WriteLine("Type quit to close server");
            while (true)
            {
                string isQuit = Console.ReadLine();
                if (isQuit == "quit")
                    break;
                else if (isQuit == "help")
                    Console.WriteLine("Type quit to close server");
            }

            matchingServer.Close();


        }
    }
}
