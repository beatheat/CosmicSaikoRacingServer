using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using EdenNetwork;
using System.Text.Json;
using System.Text.Json.Serialization;
using CSRServer.Game;
using CSRServer.Lobby;


namespace CSRServer
{
    class Program
    {
        /// <summary>
        /// 게임 시작 시 로드할 데이터의 경로모음
        /// </summary>
        public struct Config
        {
            public int port;
            public string gamelogPath;
            public string networklogPath;

            public string moduleChipPath;

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
            Console.Title = "CosmicSaikoRacing - GameServer";
            try { LoadConfig(); }
            catch (Exception e) { Console.WriteLine("Fail in Config Loading :: " + e.Message); return; }
            
            //게임서버 초기화 및 실행
            EdenNetServer server = new EdenNetServer(config.port, config.networklogPath);
            GameManager gameManager = new GameManager(server);
            
            try
            {
                Logger.Load(config.gamelogPath);
                // CardManager.Load("Data/cards.json");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            gameManager.Run(new BootScene(gameManager, server));
            
            //콘솔창 관리
            Console.WriteLine("Type quit to close server");
            while (true)
            {
                string? isQuit = Console.ReadLine();
                if (isQuit == "quit")
                    break;
                else if (isQuit == "help")
                    Console.WriteLine("Type quit to close server");
            }
            
            gameManager.Close();
            Logger.Close();
        }
    }
}