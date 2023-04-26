using EdenNetwork;
using CSRServer.Lobby;
using CSRServer.Game;
using EdenNetwork.Udp;


namespace CSRServer
{
    /// <summary>
    /// 게임 내 모든 페이즈가 공유하는 정보를 담은 클래스
    /// </summary>
    public class TurnData
    {
        // 게임에 접속한 플레이어를 리스트와 맵 두가지로 관리함
        // clientId를 id값으로 playerMap에서 GamePlayer 검색하고 클라이언트에게는 playerList만 보내줌
        public List<GamePlayer> playerList = new List<GamePlayer>();
        public Dictionary<string, GamePlayer> playerMap = new Dictionary<string, GamePlayer>();
        public int turn = 1;
    }

    public class GameScene : Scene
    {
        private readonly TurnData _turnData;

        public PreheatPhase preheatPhase = null!;
        public DepartPhase departPhase = null!;
        public MaintainPhase maintainPhase = null!;
        
        public GameScene(GameManager gameManager, EdenUdpServer server) : base(gameManager, server)
        {
            _turnData = new TurnData();
        }

        public override void Load()
        {
            if (passedData == null || passedData.ContainsKey("playerList") == false)
            {
                throw new Exception("GameScene - passedData is null or playerList does not exists");
            }
            //로비 씬으로 부터 플레이어 정보를 받아온다
            List<LobbyPlayer> lbPlayerList = (List<LobbyPlayer>)passedData["playerList"];

            foreach(LobbyPlayer lbPlayer in lbPlayerList)
            {
                GamePlayer gamePlayer = new GamePlayer(lbPlayer.clientId, _turnData.playerList.Count, lbPlayer.nickname, _turnData.playerList, this);
                _turnData.playerList.Add(gamePlayer);
                _turnData.playerMap.Add(gamePlayer.clientId, gamePlayer);
            }

            preheatPhase = new PreheatPhase(gameManager, server, _turnData, this);
            departPhase = new DepartPhase(gameManager, server, _turnData, this);
            maintainPhase = new MaintainPhase(gameManager, server, _turnData, this);

            server.AddReceiveEvent("PlayerReady", PlayerReady);
            server.BroadcastAsync("LobbyGameStart");
        }

        public override void Destroy()
        {
            server.RemoveReceiveEvent("PlayerReady");
        }
        
        public void PreheatStart()
        {
            preheatPhase.PreheatStart();
        }

        public void DepartStart()
        {
            departPhase.DepartStart();
        }

        public void MaintainStart()
        {
            maintainPhase.MaintainStart();
        }
        
        /// <summary>
        /// 클라이언트 플레이어가 본인의 플레이어 정보 외 다른 플레이어 정보를 원할때 보내주는 플레이어 리스트를 반환한다
        /// </summary>
        public List<GamePlayer> GetMonitorPlayerList()
        {
            List<GamePlayer> monitorPlayerList = new List<GamePlayer>();
            foreach (var player in _turnData.playerList)
            {
                monitorPlayerList.Add(player.CloneForMonitor());
            }
            return monitorPlayerList;
        }

        #region Receive/Response Methods
        /// <summary>
        /// 클라이언트가 게임시작 준비가 되었음을 받는 메소드
        /// </summary>
        private void PlayerReady(string clientId, EdenData data)
        {
            _turnData.playerMap[clientId].phaseReady = true;
            bool gameStart = true;
            foreach (var player in _turnData.playerList)
            {
                gameStart = gameStart && player.phaseReady;
            }

            if (gameStart)
            {
                _turnData.turn = 1;
                PreheatStart();
            }
        }
        #endregion
    }
}
