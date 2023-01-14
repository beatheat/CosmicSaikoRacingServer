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
        }

        public void RemoveGameClient(string client_id)
        {
            clients.Remove(client_id);
        }

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
        public void Run()
        {
            server.Listen(MAX_PLAYER, (string client_id) =>
            {
                if (!clients.ContainsKey(client_id))
                    clients.Add(client_id, new GameClient(client_id));
                else //클라 재접속
                {

                }
            });

            scenes.Enqueue(new LobbyScene(this, server));

            overlayScene = new ChatScene(this, server);
            overlayScene.Load();

            Scene scene = scenes.Peek();
            scene.Load();
        }

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
            nextScene.passingData = pastScene.passingData;
            pastScene.Destroy();
            nextScene.Load();
        }

        public void EndScene()
        {
            scenes.Dequeue().Destroy();
        }
        public void Log(string log)
        {
            log = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ff") + "|EdenGameManager]" + log;
            Console.WriteLine(log);
            logStream.WriteLine(log);
        }
    }
}
