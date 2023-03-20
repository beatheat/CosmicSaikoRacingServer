using EdenNetwork;

namespace CSRServer.Game
{
    internal class PreheatPhase
    {

        private const int INITIAL_TIME = 99;
        
        private readonly TurnData _turnData;
        private readonly GameManager _gameManager;
        private readonly EdenNetServer _server;
        private readonly GameScene _parent;

        private Timer? _timer;
        private int _time;

        public PreheatPhase(GameManager gameManager, EdenNetServer server, TurnData turnData, GameScene parent)
        {
            this._gameManager = gameManager;
            this._server = server;
            this._turnData = turnData;
            this._parent = parent;
            this._timer = null;
            this._time = 0;
        }

    
        public void PreheatStart()
        {
            _time = INITIAL_TIME;
            foreach (var player in _turnData.playerList)
            {
                player.PreheatStart();
            }

            //TurnStart로 대체하자 -> playerList, playerIndex, turn
            var monitorPlayerList = _parent.GetMonitorPlayerList();
            foreach (var player in _turnData.playerList)
            {
                _server.SendAsync("PreheatStart", player.clientId, new Dictionary<string, object>
                {
                    ["player"] = player,
                    ["playerList"] = monitorPlayerList,
                    ["turn"] = _turnData.turn,
                    ["timer"] = _time
                });
            }

            _timer = new Timer(GameTimer, null, 0, 1000);
            _server.AddResponse("UseCard", UseCard);
            _server.AddResponse("RerollResource", RerollResource);
            _server.AddReceiveEvent("PreheatReady", PreheatReady);

        }

        private void PreheatEnd()
        {
            _timer?.Dispose();
            _server.RemoveResponse("UseCard");
            _server.RemoveResponse("RerollResource");
            _server.AddReceiveEvent("PreheatReady", PreheatReady);
            _parent.DepartStart();
        }
        
        // 1초에 한번씩 실행함
        private void GameTimer(object? sender)
        {
            if (_time >= 0)
            {
                _time--;
                _server.BroadcastAsync("PreheatTime", _time);
            }
            else
            {
                PreheatEnd();
            }
        }

        private void PreheatReady(string clientId, EdenData data)
        {
            GamePlayer player = _turnData.playerMap[clientId];
            player.turnReady = true;

            //모든 플레이어가 예열턴을 마쳤는지 체크
            bool checkAllReady = true;
            foreach (var p in _turnData.playerList)
                checkAllReady = checkAllReady && p.turnReady;
            if (checkAllReady)
                PreheatEnd();

        }

        private EdenData UseCard(string clientId, EdenData data)
        {
            GamePlayer player = _turnData.playerMap[clientId];
            if (player.turnReady)
                return EdenData.Error("UseCard - Player turn ends");
            if (!data.TryGet<int>(out var useCardIndex))
                return EdenData.Error("UseCard - Card index is missing");
            if (!player.IsCardEnable(useCardIndex))
                return EdenData.Error("UseCard - Card does not satisfy resource condition");

            if (!player.UseCard(useCardIndex, out var result))
                return new EdenData(new EdenError("UseCard - Cannot use card"));
            return new EdenData(new Dictionary<string, object> {["player"] = player, ["results"] = result});

        }

        private EdenData RerollResource(string clientId, EdenData data)
        {

            GamePlayer player = _turnData.playerMap[clientId];
            if (player.turnReady)
                return EdenData.Error("RollResource - Player turn ends");
            if (!data.TryGet<List<int>>(out var resourceFixed))
                return EdenData.Error("RollResource - resourceFixed data is missing");
            var result = player.RollResource(resourceFixed);
            if (result == null)
                return EdenData.Error("RollResource - Reroll Count is 0");
            return new EdenData(player);
        }

    }
}