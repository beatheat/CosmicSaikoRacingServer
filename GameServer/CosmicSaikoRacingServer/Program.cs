﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using EdenNetwork;
using System.Text.Json;
using System.Text.Json.Serialization;
using CSRServer.Game;
using CSRServer.Lobby;
using Mono.Nat;


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
            

            try
            {
                //게임서버 초기화 및 실행
                // while (!EdenNetServer.IsPortAvailable(config.port))
                //     config.port++;
                _server = new EdenNetServer(config.port, config.networklogPath);
                _gameManager = new GameManager(_server);
                Logger.Load(config.gamelogPath);
                CardManager.Load(config.cardDataPath);
                
                //UPnP적용
                NatUtility.DeviceFound += DeviceFound;
                NatUtility.StartDiscovery ();
                //Upnp 디바이스 찾기 10초 타임아웃
                Task.Delay(TimeSpan.FromMilliseconds(10000)).ContinueWith(_ => NatUtility.StopDiscovery());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Close();
                return;
            }

            //게임서버 시작
            _gameManager.Run(new BootScene(_gameManager, _server));
            
            //콘솔창 관리    
            Console.WriteLine("Type quit to close server");
            while (true)
            {
                string? isQuit = Console.ReadLine()?.ToLower();
                if (isQuit == "quit")
                    break;
                //서버 재시작
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
        
        //UPnP로 (internal port-external port)=16969:16969적용
        private static async void DeviceFound(object? sender, DeviceEventArgs args)
        {
            INatDevice device = args.Device;

            var mapping = new Mapping(Protocol.Tcp, config.port, config.port);
            mapping = await device.CreatePortMapAsync(mapping);
            Logger.Log ($"Create Mapping: protocol={mapping.Protocol}, public={mapping.PublicPort}, private={mapping.PrivatePort}");
            NatUtility.StopDiscovery();
        }
    }
}
