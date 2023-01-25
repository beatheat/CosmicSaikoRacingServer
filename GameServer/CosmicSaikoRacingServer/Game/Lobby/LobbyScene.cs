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
        private Dictionary<string, LobbyPlayer> playerMap;
        private int roomNumber;
        private string hostId;

        public LobbyScene(GameManager gameManager, EdenNetServer server) : base(gameManager, server)
        {
            playerList = new List<LobbyPlayer>();
            playerMap = new Dictionary<string, LobbyPlayer>();
            roomNumber = 0;
            hostId = "";
        }

        public override void Load()
        {
            server.AddReceiveEvent("csLobbyLogin", Login);
            server.AddReceiveEvent("csLobbyLogout", Logout);
            server.AddReceiveEvent("csLobbyReady", Ready);
            server.AddReceiveEvent("csLobbyGameStart", GameStart);
            server.SetClientDisconnectEvent(RemovePlayer);

            //Create LobbyPlayer instance of host player
            roomNumber = (int)passedData["roomNumber"];
            hostId = (string)passedData["hostId"];
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
            gameManager.ChangeToNextScene();
        }

        private void RemovePlayer(string clientId)
        {
            if (playerMap.ContainsKey(clientId))
            {
                playerList.Remove(playerMap[clientId]);
                playerMap.Remove(clientId);
                gameManager.RemoveGameClient(clientId);
            }
        }

        #region Network Methods
        private void Login(string clientId, EdenData data)
        {

            string nickname = data.Get<string>();
            LobbyPlayer player = new LobbyPlayer(clientId, nickname, playerList.Count);
            playerMap.Add(clientId, player);
            if (clientId == hostId)
            {
                player.host = true;
                playerList.Insert(0, player);
            }
            else
                playerList.Add(player);
            server.Send("scLobbyPlayerId", clientId, player.id);
            server.Send("scLobbyNumber", clientId, roomNumber);
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
            playerMap[clientId].isReady = isReady;
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
