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
    public class LobbyScene : Scene
    {
        // 로비에 접속한 플레이어를 리스트와 맵 두가지로 관리함
        // clientId를 id값으로 playerMap에서 LobbyPlayer를 검색하고 클라이언트에게는 playerList만 보내줌
        private readonly List<LobbyPlayer> _playerList;
        private readonly Dictionary<string, LobbyPlayer> _playerMap;
        //로비의 방번호
        private int _roomNumber;
        //로비 호스트의 clientId
        private string _hostId;
        
        
        public LobbyScene(GameManager gameManager, EdenNetServer server) : base(gameManager, server)
        {
            _playerList = new List<LobbyPlayer>();
            _playerMap = new Dictionary<string, LobbyPlayer>();
            _roomNumber = 0;
            _hostId = "";
        }

        public override void Load()
        {
            //로비 호스트의 clientId값과 방번호를 부트씬으로부터 받아온다
            if (passedData == null)
            {
                throw new Exception("LobbyScene - passedData is null");
            }
            _roomNumber = (int)passedData["roomNumber"];
            _hostId = (string)passedData["hostId"];
            
            server.AddResponse("LobbyLogin", Login);
            server.AddReceiveEvent("LobbyLogout", Logout);
            server.AddReceiveEvent("LobbyReady", Ready);
            server.AddReceiveEvent("LobbyGameStart", GameStart);
            server.SetClientDisconnectEvent(RemovePlayer);
        }

        public override void Destroy()
        {
            server.RemoveResponse("LobbyLogin");
            server.RemoveReceiveEvent("LobbyLogout");
            server.RemoveReceiveEvent("LobbyReady");
            server.RemoveReceiveEvent("LobbyGameStart");
            server.ResetClientDisconnectEvent();
        }
        
        #region Lobby Logic Methods
        private void ChangeScene()
        {
            passingData.Add("playerList", _playerList);
            gameManager.ChangeToNextScene();
        }
        
        /// <summary>
        /// 로비에서 클라이언트 접속이 끊어질 시 해당 클라이언트 리스트에서 제거한다.
        /// </summary>
        private void RemovePlayer(string clientId)
        {
            if (_playerMap.ContainsKey(clientId))
            {
                _playerList.Remove(_playerMap[clientId]);
                _playerMap.Remove(clientId);
                gameManager.RemoveGameClient(clientId);
            }
            server.BroadcastAsync("LobbyPlayerUpdate", _playerList);
        }
        #endregion
        #region Receive/Response Methods
        /// <summary>
        /// 클라이언트가 최초에 로비에 접속한 뒤 필요한 로비정보를 응답해준다
        /// </summary>
        private EdenData Login(string clientId, EdenData data)
        {
            if (!data.TryGet<string>(out var nickname))
                return new EdenData(new EdenError("Login nickname is missing"));
            LobbyPlayer player = new LobbyPlayer(clientId, nickname, _playerList.Count);
            _playerMap.Add(clientId, player);
            if (clientId == _hostId)
            {
                player.host = true;
                _playerList.Insert(0, player);
            }
            else
                _playerList.Add(player);

            server.BroadcastExceptAsync("LobbyPlayerUpdate", clientId, _playerList);

            var response = new Dictionary<string, object>
            {
                ["playerId"] = player.id,
                ["lobbyNumber"] = _roomNumber,
                ["playerList"] = _playerList
            };
            return new EdenData(response);
        }

        /// <summary>
        /// 클라이언트가 로비를 나감을 알린다
        /// </summary>
        private void Logout(string clientId, EdenData data)
        {
            RemovePlayer(clientId);
        }

        /// <summary>
        /// 클라이언트가 게임준비를 했음을 알린다
        /// </summary>
        private void Ready(string clientId, EdenData data)
        {
            if (!data.TryGet<bool>(out var isReady))
                return;
            LobbyPlayer player = _playerMap[clientId];
            player.isReady = isReady;
            server.BroadcastAsync("LobbyPlayerReady", new Dictionary<string, object> {
                ["playerId"] = player.id,
                ["readyState"] = isReady
            });
        }
        
        /// <summary>
        /// 모든 클라이언트가 준비완료하고 호스트가 게임을 시작했을 때 게임을 시작한다
        /// </summary>
        private void GameStart(string clientId, EdenData data)
        {
            foreach (var player in _playerList)
            {
                if (!player.isReady)
                    return;
            }
            ChangeScene();
        }


        #endregion
    }
}
