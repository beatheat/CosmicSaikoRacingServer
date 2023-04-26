using EdenNetwork;
using EdenNetwork.Udp;

namespace CSRServer.Game
{
    public class PreheatPhase
    {
        private const int INITIAL_TIME = 99;
        
        private readonly TurnData _turnData;
        private readonly GameManager _gameManager;
        private readonly EdenUdpServer _server;
        private readonly GameScene _parent;

        private Timer? _timer;
        private int _time;
        private bool _turnEnd;

        public PreheatPhase(GameManager gameManager, EdenUdpServer server, TurnData turnData, GameScene parent)
        {
            this._gameManager = gameManager;
            this._server = server;
            this._turnData = turnData;
            this._parent = parent;
            this._timer = null;
            this._time = 0;
            this._turnEnd = false;
        }

        /// <summary>
        /// 예열 페이즈 시작
        /// </summary>
        public void PreheatStart()
        {
            _server.AddResponse("UseCard", UseCard);
            _server.AddResponse("RerollResource", RerollResource);
            _server.AddReceiveEvent("PreheatReady", PreheatReady);
            
            _time = INITIAL_TIME;
            _turnEnd = false;
            
            foreach (var player in _turnData.playerList)
            {
                player.PreheatStart();
            }
            
            var orderedPlayerList = _turnData.playerList.OrderByDescending(p => p.currentDistance).ToList();
            //플레이어 rank 정해주기
            for (int i = 0; i < orderedPlayerList.Count; i++)
                orderedPlayerList[i].rank = i + 1;

            // 예열 페이즈 시작시 턴 데이터 클라이언트와 동기화
            var monitorPlayerList = _parent.GetMonitorPlayerList();
            foreach (var player in _turnData.playerList)
            {
                _server.Send("PreheatStart", player.clientId, new Dictionary<string, object>
                {
                    ["player"] = player,
                    ["playerList"] = monitorPlayerList,
                    ["turn"] = _turnData.turn,
                    ["timer"] = _time
                });
            }

            _timer = new Timer(GameTimer, null, 0, 1000);
        }

        /// <summary>
        /// 예열 페이즈 준비완료
        /// </summary>
        public void Ready(GamePlayer player)
        {
            player.phaseReady = true;

            //모든 플레이어가 예열페이즈를 마쳤는지 체크
            bool checkAllReady = true;
            foreach (var p in _turnData.playerList)
                checkAllReady = checkAllReady && p.phaseReady;
            //모든 플레이어가 예열페이즈를 마쳤다면 예열페이즈 종료
            if (checkAllReady)
                PreheatEnd();
        }

        /// <summary>
        /// 예열 페이즈 종료
        /// </summary>
        private void PreheatEnd()
        {
            lock (this)
            {
                if (_turnEnd == false)
                {
                    _timer?.Dispose();
                    _server.RemoveResponse("UseCard");
                    _server.RemoveResponse("RerollResource");
                    _server.RemoveReceiveEvent("PreheatReady");
                    foreach (var player in _turnData.playerList)
                        player.PreheatEnd();
                    _parent.DepartStart();
                    _turnEnd = true;
                }
            }
        }
        
        /// <summary>
        /// 예열 페이즈 타이머
        /// </summary>
        private async void GameTimer(object? sender)
        {
            if (_time > 0)
            {
                _time--;
                await _server.BroadcastAsync("PreheatTime", _time, log: false);
            }
            else
            {
                PreheatEnd();
            }
        }

        #region Receive/Response Methods
        /// <summary>
        /// 예열턴 준비완료 API 
        /// TurnEnd 효과 때문에 public으로 선언
        /// </summary>
        private void PreheatReady(string clientId, EdenData data)
        {
            GamePlayer player = _turnData.playerMap[clientId];
            Ready(player);
        }

        /// <summary>
        /// 카드 사용 API
        /// </summary>
        private EdenData UseCard(string clientId, EdenData data)
        {
            GamePlayer player = _turnData.playerMap[clientId];
            if (player.phaseReady)
                return EdenData.Error("UseCard - Player turn ends");
            if (!data.TryGet<int>(out var useCardIndex))
                return EdenData.Error("UseCard - Card index is missing");
            if (!player.cardSystem.IsCardEnable(useCardIndex))
                return EdenData.Error("UseCard - Card does not satisfy resource condition");

            if (!player.cardSystem.UseCard(useCardIndex, out var result))
                return new EdenData(new EdenError("UseCard - Cannot use card"));
            return new EdenData(new Dictionary<string, object> {["player"] = player, ["results"] = result});

        }

        /// <summary>
        /// 리롤 리소스 API
        /// </summary>
        private EdenData RerollResource(string clientId, EdenData data)
        {

            GamePlayer player = _turnData.playerMap[clientId];
            if (player.phaseReady)
                return EdenData.Error("RollResource - Player turn ends");
            if (!data.TryGet<List<int>>(out var resourceFixed))
                return EdenData.Error("RollResource - resourceFixed data is missing");
            var result = player.resourceSystem.RerollResource(resourceFixed);
            if (result == null)
                return EdenData.Error("RollResource - Reroll Count is 0");
            return new EdenData(player);
        }
        #endregion
    }
}