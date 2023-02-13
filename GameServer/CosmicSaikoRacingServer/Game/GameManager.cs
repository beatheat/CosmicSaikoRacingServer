using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdenNetwork;

namespace CSRServer
{
    public class GameManager
    {

        public const int MAX_PLAYER = 8;
        private EdenNetServer server;
        private Dictionary<string, GameClient> clients;
        private Queue<Scene> scenes;
        private Scene? overlayScene;

        private StreamWriter logStream;
        private Thread logThread;


        public GameManager(EdenNetServer server)
        {
            this.server = server;
            this.clients = new Dictionary<string, GameClient>();
            this.scenes = new Queue<Scene>();
            this.overlayScene = null;
            logThread = null!;
            logStream = null!;
        }

        public void RemoveGameClient(string client_id)
        {
            clients.Remove(client_id);
        }

        /// <summary>
        /// 게임서버 최초 로딩, 로그 준비
        /// </summary>
        public void Load()
        {
            try
            {
                logStream = new StreamWriter(Program.config.gamelogPath, append: true);
                logThread = new Thread(() =>
                {
                    try
                    {
                        while (logStream.BaseStream != null)
                        {
                            Thread.Sleep(3 * 60 * 1000);
                            logStream.Flush();
                        }
                    }
                    catch //(ThreadInterruptedException e)
                    {
                        Console.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ff") + "|EdenGameManager]" + "Log stream is closed");
                    }
                });
                logThread.Start();
            }
            catch (Exception e)
            {
                throw new Exception("EdenGameManager::Load - Cannot create log-file stream \n" + e.Message);
            }
        }

        /// <summary>
        /// 게임서버를 실행함
        /// </summary>
        public void Run()
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

            server.AddResponse("IsGameReady", (string client_id, EdenData d) => 
            {
                return new EdenData(scenes.Count > 0);
            });

            // 랜덤 매칭 게임 생성
            server.AddResponse("CreateGame", (string client_id, EdenData d) =>
            {

                //Scene scene = scenes.Peek();
                //scene.passingData.Add("hostplayer", new LobbyPlayer(client_id, d.Get<string>(), 0, true));
                //scene.passingData.Add("roomNumber", roomNumber);
                //overlayScene = new ChatScene(this, server);
                //overlayScene.Load();
                //return new EdenData(1, "OK");
                Task.Run(() => 
                { 
                    //while(server.Client)
                });
                return new EdenData();
            });

            // 커스텀 게임 생성
            server.AddResponse("CreateCustomGame", (string client_id, EdenData d) =>
            {
                EdenNetClient client = new EdenNetClient(Program.config.matchingServerAddress, Program.config.matchingServerPort);
                if (client.Connect() == ConnectionState.OK)
                {
                    EdenData data = client.Request("CreateLobby", 10, Program.config.port);
                    if(data.type == EdenData.Type.ERROR)
                    {
                        return new EdenData(new Dictionary<string, object>
                        {
                            ["state"] = false,
                            ["message"] = "Cannot Create Game : cannot create lobby"
                        });
                    }
                    int roomNumber = data.Get<int>();

                    scenes.Enqueue(new LobbyScene(this, server));

                    Scene scene = scenes.Peek();
                    scene.passedData = new Dictionary<string, object>();
                    scene.passedData.Add("hostId", client_id);
                    scene.passedData.Add("roomNumber", roomNumber);
                    
                    scene.Load();
                    overlayScene = new ChatScene(this, server);
                    overlayScene.Load();

                    return new EdenData(new Dictionary<string, object>
                    {
                        ["state"] = true,
                        ["message"] = "OK"
                    });
                }
                return new EdenData(new Dictionary<string, object>
                {
                    ["state"] = false,
                    ["message"] = "Cannot Create Game : cannot connect Matching Server"
                });
            });

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
            if (overlayScene != null)
                overlayScene.Destroy();
            logThread.Interrupt();
            logStream.Close();
            server.Close();
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
            catch //(InvalidOperationException e)
            {
                Console.WriteLine("GameManger::ChangeToNextScene called in last scene - " + pastScene.GetType().Name);
                return;
            }
            nextScene.passedData = pastScene.passingData;
            pastScene.Destroy();
            nextScene.Load();
        }

        public void EndScene()
        {
            scenes.Dequeue().Destroy();
        }
        /// <summary>
        /// 게임서버 로그를 출력함
        /// </summary>
        public void Log(string log)
        {
            log = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ff") + "|EdenGameManager]" + log;
            Console.WriteLine(log);
            logStream.WriteLine(log);
        }
    }
}
