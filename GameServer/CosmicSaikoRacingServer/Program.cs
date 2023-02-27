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

            public string cardDataPath;
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

        private static EdenNetServer? _server;
        private static GameManager? _gameManager;

        static void Close()
        {
            _gameManager?.Close();
            _server?.Close();
            Logger.Close();
        }
        
        static void Main(string[] args)
        {
            Console.Title = "CosmicSaikoRacing - GameServer";
            try { LoadConfig(); }
            catch (Exception e) { Console.WriteLine("Fail in Config Loading :: " + e.Message); return; }
            
            //게임서버 초기화 및 실행
            _server = new EdenNetServer(config.port, config.networklogPath);
            _gameManager = new GameManager(_server);

            //데이터 로딩
            try
            {
                Logger.Load(config.gamelogPath);
                CardManager.Load(config.cardDataPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Close();
                return;
            }

            _gameManager.Run(new BootScene(_gameManager, _server));
            
            //콘솔창 관리
            Console.WriteLine("Type quit to close server");
            while (true)
            {
                string? isQuit = Console.ReadLine();
                if (isQuit == "quit")
                    break;
                if (isQuit == "r")
                {
                    _gameManager.Close();
                    _server.Close();
                    _server = new EdenNetServer(config.port, config.networklogPath);
                    _gameManager = new GameManager(_server);
                    _gameManager.Run(new BootScene(_gameManager, _server));
                    Console.WriteLine("Server restarted");
                }
                else if (isQuit == "help")
                    Console.WriteLine("Type quit to close server");
            }

            Close();
        }
    }
}
