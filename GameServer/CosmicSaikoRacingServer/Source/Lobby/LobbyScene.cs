using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using EdenNetwork;
using CSRServer;
using CSRServer.Lobby;
using System.Drawing;
using System.Numerics;

namespace CSRServer
{
    /// <summary>
    /// CSR의 첫번째 씬인 로비씬
    /// </summary>
    public class LobbyScene : Scene
    {
        #region Properties
        // 로비에 접속한 플레이어를 리스트와 맵 두가지로 관리함
        // clientId를 id값으로 playerMap에서 LobbyPlayer를 검색하고 클라이언트에게는 playerList만 보내줌
        private readonly List<LobbyPlayer> playerList;
        private readonly Dictionary<string, LobbyPlayer> playerMap;
        //로비의 방번호
        private int roomNumber;
        //로비 호스트의 clientId
        private string hostId;
        #endregion
        
        
        #region Load Methods
        public LobbyScene(GameManager gameManager, EdenNetServer server) : base(gameManager, server)
        {
            playerList = new List<LobbyPlayer>();
            playerMap = new Dictionary<string, LobbyPlayer>();
            roomNumber = 0;
            hostId = "";
        }

        public override void Load()
        {
            server.AddResponse("LobbyLogin", Login);
            server.AddReceiveEvent("LobbyLogout", Logout);
            server.AddReceiveEvent("LobbyReady", Ready);
            server.AddReceiveEvent("LobbyGameStart", GameStart);
            server.SetClientDisconnectEvent(RemovePlayer);

            //로비 호스트의 clientId값과 방번호를 
            if (passedData == null)
            {
                throw new Exception("LobbyScene - passedData is null");
            }
            roomNumber = (int)passedData["roomNumber"];
            hostId = (string)passedData["hostId"];
        }

        public override void Destroy()
        {
            server.RemoveResponse("LobbyLogin");
            server.RemoveReceiveEvent("LobbyLogout");
            server.RemoveReceiveEvent("LobbyReady");
            server.RemoveReceiveEvent("LobbyGameStart");
            server.ResetClientDisconnectEvent();
        }

        #endregion
        #region Lobby Logic Methods
        private void ChangeScene()
        {
            passingData.Add("playerList", playerList);
            gameManager.ChangeToNextScene();
        }

        //로비에서 클라이언트 접속이 끊어질 시 해당 클라이언트 리스트에서 제거
        private void RemovePlayer(string clientId)
        {
            if (playerMap.ContainsKey(clientId))
            {
                playerList.Remove(playerMap[clientId]);
                playerMap.Remove(clientId);
                gameManager.RemoveGameClient(clientId);
            }
            server.BroadcastAsync("LobbyPlayerUpdate", playerList);
        }
        #endregion
        #region Receive/Response Methods
        // 클라이언트가 최초에 로비에 접속한 뒤 필요한 로비정보를 응답해줌
        private EdenData Login(string clientId, EdenData data)
        {
            if (!data.TryGet<string>(out var nickname))
                return new EdenData(new EdenError("Login nickname is missing"));
            LobbyPlayer player = new LobbyPlayer(clientId, nickname, playerList.Count);
            playerMap.Add(clientId, player);
            if (clientId == hostId)
            {
                player.host = true;
                playerList.Insert(0, player);
            }
            else
                playerList.Add(player);

            server.BroadcastExceptAsync("LobbyPlayerUpdate", clientId, playerList);

            var response = new Dictionary<string, object>
            {
                ["playerId"] = player.id,
                ["lobbyNumber"] = roomNumber,
                ["playerList"] = playerList
            };
            return new EdenData(response);
        }

        // 클라이언트가 로비를 나감을 알림
        private void Logout(string clientId, EdenData data)
        {
            RemovePlayer(clientId);
        }

        // 클라이언트가 게임준비를 했음을 알림
        private void Ready(string clientId, EdenData data)
        {
            if (!data.TryGet<bool>(out var isReady))
                return;
            LobbyPlayer player = playerMap[clientId];
            player.isReady = isReady;
            server.BroadcastAsync("LobbyPlayerReady", new Dictionary<string, object> {
                ["playerId"] = player.id,
                ["readyState"] = isReady
            });
        }

        // 모든 클라이언트가 준비완료하고 호스트가 게임을 시작했을 때 게임을 시작함
        private void GameStart(string clientId, EdenData data)
        {
            foreach (var player in playerList)
            {
                if (!player.isReady)
                    return;
            }
            server.BroadcastAsync("LobbyGameStart");
            ChangeScene();
        }


        #endregion
    }
}
