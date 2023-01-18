using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using EdenNetwork;
using System.Text.Json;



namespace CSRServer
{
    class Program
    {

        public struct Config
        {
            public int port;
            public string gamelogPath;
            public string networklogPath;

            public string matchingServerAddress;
            public int matchingServerPort;
        }
        public static Config config;
        static void LoadConfig()
        {
            string configStr;
            try { configStr = File.ReadAllText("Config.json"); }
            catch { throw new Exception("Config.json is missing"); }
            try { config = JsonSerializer.Deserialize<Config>(configStr, new JsonSerializerOptions { IncludeFields = true }); }
            catch { throw new Exception("Config.json is not formatted"); }
        }

        static void Main(string[] args)
        {
            try { LoadConfig(); }
            catch (Exception e) { Console.WriteLine("Fail in Config Loading :: " + e.Message); return; }

            EdenNetServer server = new EdenNetServer(config.port, config.networklogPath);
            GameManager gameManager = new GameManager(server);

            try { gameManager.Load(); }
            catch (Exception e) { Console.WriteLine("Fail in Server Loading \n" + e.Message); return; }

            gameManager.Run();

            Console.WriteLine("Type quit to close server");
            while (true)
            {
                string isQuit = Console.ReadLine();
                if (isQuit == "quit")
                    break;
                else if (isQuit == "help")
                    Console.WriteLine("Type quit to close server");
            }

            gameManager.Close();

        }
    }
}