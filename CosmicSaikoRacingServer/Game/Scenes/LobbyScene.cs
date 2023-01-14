using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using EdenNetwork;
using CSRServer;
using System.Drawing;

namespace CSRServer
{

    public class LobbyScene : Scene
    {
        public const string Name = "Lobby";

        private List<LobbyPlayer> playerList;
        private Dictionary<string, int> cid2idx;


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
        }

        public override void Destroy()
        {
            server.RemoveReceiveEvent("csLobbyLogin");
            server.RemoveReceiveEvent("csLobbyLogout");
            server.RemoveReceiveEvent("csLobbyReady");
            server.RemoveReceiveEvent("csLobbyGameStart");
            server.ResetClientDisconnectEvent();
        }

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

        private void RemovePlayer(string clientId)
        {
            int deleteIdx = cid2idx[clientId];
            
            playerList.RemoveAt(deleteIdx);
            for (int i = deleteIdx; i < playerList.Count; i++)
            {
                cid2idx[playerList[i].clientId] = i;
            }
            gameManager.RemoveGameClient(clientId);
        }


        private void ChangeScene()
        {
            passingData.Add("playerList", playerList);
            passingData.Add("cid2idx", cid2idx);
            gameManager.ChangeToNextScene();
        }
    }
}
