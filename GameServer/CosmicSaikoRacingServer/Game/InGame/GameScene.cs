using EdenNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;


namespace CSRServer
{
    internal class GameScene : Scene
    {
        List<GamePlayer> playerList;
        Dictionary<string, GamePlayer> playerMap;

        int turn = 0;
        int time = 0;
        Timer timer;

        public GameScene(GameManager gameManager, EdenNetServer server) : base(gameManager, server)
        {
            playerList = new List<GamePlayer>();
            playerMap = new Dictionary<string, GamePlayer>();
        }

        public override void Load()
        {
            List<LobbyPlayer> lbPlayerList = (List<LobbyPlayer>)passedData["playerList"];

            foreach(LobbyPlayer lbPlayer in lbPlayerList)
            {
                GamePlayer gamePlayer = new GamePlayer();
                gamePlayer.id = lbPlayer.id;
                gamePlayer.nickname = lbPlayer.nickname;
                gamePlayer.clientId = lbPlayer.clientId;
                playerList.Add(gamePlayer);
                playerMap.Add(gamePlayer.clientId, gamePlayer);
            }

            InitGame();
            timer = new Timer(GameThread, null, 0, 1000);
            //Thread game = new Thread(GameThread);
            //game.Start();
        }

        public override void Destroy()
        {

        }
        #region Game Methods
        public void InitGame()
        {
            turn = 0;
            time = 99;
        }
        public void GameThread(object state)
        {
            time--;
            server.Broadcast("time", time);
        }

        #endregion
        #region Network Methods

        void ChangeModuleChip()
        {

        }


        void Ready()
        {

        }

        #endregion

        //public List<Resource[]> RollResource()
        //{
        //    //List<Resource[]> resourceList = new List<Resource[]>();
        //    //for(int i=0;i<4;i++)
        //    //{
        //    //    resourceList[i]
        //    //}
        //}

    }
}
