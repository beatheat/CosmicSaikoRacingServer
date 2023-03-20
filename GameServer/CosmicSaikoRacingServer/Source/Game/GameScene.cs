using EdenNetwork;
using CSRServer.Lobby;
using CSRServer.Game;


namespace CSRServer
{

    public class TurnData
    {
        // 게임에 접속한 플레이어를 리스트와 맵 두가지로 관리함
        // clientId를 id값으로 playerMap에서 GamePlayer 검색하고 클라이언트에게는 playerList만 보내줌
        public List<GamePlayer> playerList = new List<GamePlayer>();
        public Dictionary<string, GamePlayer> playerMap = new Dictionary<string, GamePlayer>();
        public int turn = 1;
    }

    internal class GameScene : Scene
    {
        private readonly TurnData _turnData;

        private PreheatPhase _preheatPhase = null!;
        private DepartPhase _departPhase = null!;
        private MaintainPhase _maintainPhase = null!;
        

        public GameScene(GameManager gameManager, EdenNetServer server) : base(gameManager, server)
        {

            _turnData = new TurnData();
        }

        //로비 씬으로 부터 플레이어 정보를 받아옴
        public override void Load()
        {
            if (passedData == null || passedData.ContainsKey("playerList") == false)
            {
                throw new Exception("GameScene - passedData is null or playerList does not exists");
            }
            List<LobbyPlayer> lbPlayerList = (List<LobbyPlayer>)passedData["playerList"];

            foreach(LobbyPlayer lbPlayer in lbPlayerList)
            {
                GamePlayer gamePlayer = new GamePlayer(lbPlayer.clientId, _turnData.playerList.Count, lbPlayer.nickname, _turnData.playerList);
                _turnData.playerList.Add(gamePlayer);
                _turnData.playerMap.Add(gamePlayer.clientId, gamePlayer);
            }

            _preheatPhase = new PreheatPhase(gameManager, server, _turnData, this);
            _departPhase = new DepartPhase(gameManager, server, _turnData, this);
            _maintainPhase = new MaintainPhase(gameManager, server, _turnData, this);

            server.AddReceiveEvent("PlayerReady", PlayerReady);
                
            server.BroadcastAsync("LobbyGameStart");
        }

        public override void Destroy()
        {
            server.RemoveReceiveEvent("PlayerReady");
        }

        private void PlayerReady(string clientId, EdenData data)
        {
            _turnData.playerMap[clientId].turnReady = true;
            bool gameStart = true;
            foreach (var player in _turnData.playerList)
            {
                gameStart = gameStart && player.turnReady;
            }

            if (gameStart)
            {
                _turnData.turn = 1;
                PreheatStart();
            }
        }

        public void PreheatStart()
        {
            _preheatPhase.PreheatStart();
        }

        public void DepartStart()
        {
            _departPhase.DepartStart();
        }

        public void MaintainStart()
        {
            _maintainPhase.MaintainStart();
        }
        
        public List<GamePlayer> GetMonitorPlayerList()
        {
            List<GamePlayer> monitorPlayerList = new List<GamePlayer>();
            foreach (var player in _turnData.playerList)
            {
                monitorPlayerList.Add(player.CloneForMonitor());
            }
            return monitorPlayerList;
        }
    }
}
