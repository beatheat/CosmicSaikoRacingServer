﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSRServer.Game;
using EdenNetwork;

namespace CSRServer
{
    public class GameManager
    {

        public const int MAX_PLAYER = 8;
        private readonly EdenNetServer server;
        private readonly Dictionary<string, GameClient> clients;
        private readonly Queue<Scene> scenes;
        

        public GameManager(EdenNetServer server)
        {
            this.server = server;
            this.clients = new Dictionary<string, GameClient>();
            this.scenes = new Queue<Scene>();
        }

        /// <summary>
        /// 게임서버를 실행함
        /// </summary>
        public void Run(Scene firstScene)
        {
            server.Listen(MAX_PLAYER, (string client_id) =>
            {
                if(scenes.Count == 0)
                {
                    //준비안됨
                    //server.DisconnectClient(client_id);
                }
                if (!clients.ContainsKey(client_id))
                    clients.Add(client_id, new GameClient(client_id));
                else //클라 재접속
                {

                }
            });
            scenes.Enqueue(firstScene);
            firstScene.Load();
        }
        /// <summary>
        /// 게임서버 종료
        /// </summary>
        public void Close()
        {
            while (scenes.Count > 0)
            {
                Scene scene = scenes.Dequeue();
                scene.Destroy();
            }
            server.Close();
        }
        
        /// <summary>
        /// 씬 추가
        /// </summary>
        public void AddScene(params Scene[] scenes)
        {
            foreach(var scene in scenes)
                this.scenes.Enqueue(scene);
        }
        
        /// <summary>
        /// 다음 씬으로 변경함
        /// </summary>
        public void ChangeToNextScene()
        {
            Scene pastScene, nextScene;
            pastScene = scenes.Dequeue();
            try
            {
                nextScene = scenes.Peek();
            }
            catch
            {
                Console.WriteLine("GameManger::ChangeToNextScene called in last scene - " + pastScene.GetType().Name);
                return;
            }
            nextScene.passedData = pastScene.passingData;
            try
            {
                pastScene.Destroy();
                nextScene.Load();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void RemoveGameClient(string client_id)
        {
            clients.Remove(client_id);
        }
        
        public void EndScene()
        {
            scenes.Dequeue().Destroy();
        }

    }
}
