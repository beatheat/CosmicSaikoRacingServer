using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using EdenNetwork;
using CSRServer;
using System.Drawing;
using System.Numerics;

namespace CSRServer
{

    public class LobbyScene : Scene
    {
        private List<LobbyPlayer> playerList;
        private Dictionary<string, int> cid2idx;
        private int roomNumber;

        public LobbyScene(GameManager gameManager, EdenNetServer server) : base(gameManager, server)
        {
            playerList = new List<LobbyPlayer>();
            cid2idx = new Dictionary<string, int>();
        }

        public override void Load()
        {
            server.AddReceiveEvent("csLobbyLogin", Login);
            server.AddReceiveEvent("csLobbyLogout", Logout);
            server.AddReceiveEvent("csLobbyReady", Ready);
            server.AddReceiveEvent("csLobbyGameStart", GameStart);
            server.SetClientDisconnectEvent(RemovePlayer);

            //Create LobbyPlayer instance of host player
            LobbyPlayer host = (LobbyPlayer)passingData["hostplayer"];
            cid2idx.Add(host.clientId, playerList.Count);
            playerList.Add(host);
            server.Send("scLobbyPlayerId", host.clientId, host.id);
            server.Broadcast("scLobbyPlayerUpdate", playerList);

            roomNumber = (int)passingData["roomNumber"];
        }

        public override void Destroy()
        {
            server.RemoveReceiveEvent("csLobbyLogin");
            server.RemoveReceiveEvent("csLobbyLogout");
            server.RemoveReceiveEvent("csLobbyReady");
            server.RemoveReceiveEvent("csLobbyGameStart");
            server.ResetClientDisconnectEvent();
        }

        private void ChangeScene()
        {
            passingData.Add("playerList", playerList);
            passingData.Add("cid2idx", cid2idx);
            gameManager.ChangeToNextScene();
        }

        private void RemovePlayer(string clientId)
        {
            if (cid2idx.ContainsKey(clientId))
            {
                int deleteIdx = cid2idx[clientId];

                playerList.RemoveAt(deleteIdx);
                for (int i = deleteIdx; i < playerList.Count; i++)
                {
                    cid2idx[playerList[i].clientId] = i;
                }
                gameManager.RemoveGameClient(clientId);
            }
        }

        #region Network Methods
        private void Login(string clientId, EdenData data)
        {
            string nickname = data.Get<string>();
            LobbyPlayer player = new LobbyPlayer(clientId, nickname, playerList.Count);
            cid2idx.Add(clientId, playerList.Count);
            playerList.Add(player);
            server.Send("scLobbyPlayerId", clientId, player.id);
            server.Broadcast("scLobbyPlayerUpdate", playerList);
        }

        private void Logout(string clientId, EdenData data)
        {
            RemovePlayer(clientId);
            server.Broadcast("scLobbyPlayerUpdate", playerList);
        }

        private void Ready(string clientId, EdenData data)
        {
            bool isReady = data.Get<bool>();
            int cidx = cid2idx[clientId];
            playerList[cidx].isReady = isReady;
            server.Broadcast("scLobbyPlayerUpdate", playerList);
        }

        private void GameStart(string clientId, EdenData data)
        {
            foreach (var player in playerList)
            {
                if (!player.isReady)
                    return;
            }
            server.Broadcast("scLobbyGameStart");
            ChangeScene();
        }


        #endregion
    }
}
