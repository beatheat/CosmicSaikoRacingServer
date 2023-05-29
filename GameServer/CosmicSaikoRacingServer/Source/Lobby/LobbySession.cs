using CSR.DataTransmission;
using CSR.Game.GameObject;
using EdenNetwork;

namespace CSR.Lobby;

public class LobbySession : SessionBase
{
        private readonly List<LobbyPlayer> _playerList;
        private readonly EdenUdpClient _matchClient;
        private readonly int _lobbyNumber;
        
        public LobbySession(SessionManager sessionManager, EdenUdpServer server, EdenUdpClient matchClient, int lobbyNumber) : base(sessionManager, server)
        {
            _playerList = new List<LobbyPlayer>();
            _matchClient = matchClient;
            _lobbyNumber = lobbyNumber;
        }

        public override void Load()
        {
            server.AddEndpoints(this);
        }

        public override void Destroy()
        {
            server.RemoveEndpoints(this);
        }
        

        /// <summary>
        /// 클라이언트가 최초에 로비에 접속한 뒤 필요한 로비정보를 응답해준다
        /// </summary>
        [EdenResponse]
        private Response_LobbyLogin LobbyLogin(PeerId clientId, Request_LobbyLogin request)
        {
            var player = new LobbyPlayer(clientId, request.Nickname);
            // 서버 호스트는 리스트에 첫번째로 배치한다
            if (clientId.Ip == "127.0.0.1")
            {
                player.Host = true;
                _playerList.Insert(0, player);
            }
            else
                _playerList.Add(player);

            server.BroadcastExceptAsync("LobbyPlayerUpdate", clientId, new Packet_LobbyPlayerUpdate
            {
                LobbyPlayers = _playerList
            });
            
            return new Response_LobbyLogin
            {
                PlayerId = player.Id,
                LobbyNumber = _lobbyNumber,
                LobbyPlayers =_playerList
            };
        }

        /// <summary>
        /// 클라이언트가 로비를 나감을 알린다
        /// </summary
        [EdenReceive]
        private void LobbyLogout(PeerId clientId)
        {
            var player = _playerList.Find(player => player.ClientId == clientId);
            if (player != null)
            {
                _playerList.Remove(player);
                server.BroadcastAsync("LobbyPlayerUpdate", new Packet_LobbyPlayerUpdate {LobbyPlayers = _playerList});
            }
        }

        /// <summary>
        /// 클라이언트가 게임준비를 했음을 알린다
        /// </summary>
        [EdenReceive]
        private void LobbyReady(PeerId clientId, Packet_LobbyReady packet)
        {
            var player = _playerList.Find(player => player.ClientId == clientId);
            if (player != null)
            {
                player.IsReady = packet.IsReady;
                server.BroadcastAsync("LobbyPlayerReady", new Packet_LobbyPlayerReady {PlayerId = player.Id, ReadyState = player.IsReady});
            }
        }
        
        /// <summary>
        /// 모든 클라이언트가 준비완료하고 호스트가 게임을 시작했을 때 게임을 시작한다
        /// </summary>
        [EdenReceive]
        private void LobbyGameStart(PeerId clientId)
        {
            foreach (var player in _playerList)
            {
                // 게임시작을 호스트가 보내지 않았을 경우
                if (player.ClientId == clientId && !player.Host)
                    return;
                // 모든 플레이어가 준비하지 않았을 경우
                if (!player.IsReady)
                    return;
            }

            // 매치서버에서 로비를 삭제하라는 요청을 보낸다
            Task.Run(() =>
            {
                _matchClient.Request<int>("DestroyLobby", _lobbyNumber);
                _matchClient.Close();
            });

            sessionManager.ChangeSession<GameSession>(_playerList);
            Destroy();
        }


}