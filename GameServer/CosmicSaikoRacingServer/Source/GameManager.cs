using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSRServer.Game;
using EdenNetwork;

namespace CSRServer
{
    /// <summary>
    /// 게임서버의 모든 씬을 컨트롤하는 클래스
    /// </summary>
    public class GameManager
    {
        public const int MAX_PLAYER = 8;
        private readonly EdenNetServer _server;
        private readonly Dictionary<string, GameClient> _clients;
        private readonly Queue<Scene> _scenes;
        

        public GameManager(EdenNetServer server)
        {
            this._server = server;
            this._clients = new Dictionary<string, GameClient>();
            this._scenes = new Queue<Scene>();
        }

        /// <summary>
        /// 게임서버를 실행한다
        /// </summary>
        public void Run(Scene firstScene)
        {
            // TODO: 이미 접속된 클라이언트의 연결이 끊어진 후 재접속했을 때 정보를 유지해야한다. 
            _server.Listen(MAX_PLAYER, (string clientId) =>
            {
                if(_scenes.Count == 0)
                {
                    //준비안됨
                    //server.DisconnectClient(client_id);
                }
                if (!_clients.ContainsKey(clientId))
                    _clients.Add(clientId, new GameClient(clientId));
                else //클라 재접속
                {

                }
            });
            _scenes.Enqueue(firstScene);
            firstScene.Load();
        }
        /// <summary>
        /// 게임서버 종료한다
        /// </summary>
        public void Close()
        {
            while (_scenes.Count > 0)
            {
                Scene scene = _scenes.Dequeue();
                scene.Destroy();
            }
        }
        
        /// <summary>
        /// 씬을 추가한다
        /// </summary>
        public void AddScene(params Scene[] scenes)
        {
            foreach(var scene in scenes)
                this._scenes.Enqueue(scene);
        }
        
        /// <summary>
        /// 다음 씬으로 변경한다
        /// </summary>
        public void ChangeToNextScene()
        {
            Scene pastScene, nextScene;
            pastScene = _scenes.Dequeue();
            try
            {
                nextScene = _scenes.Peek();
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
            _clients.Remove(client_id);
        }
        
        public void EndScene()
        {
            _scenes.Dequeue().Destroy();
        }

    }
}
