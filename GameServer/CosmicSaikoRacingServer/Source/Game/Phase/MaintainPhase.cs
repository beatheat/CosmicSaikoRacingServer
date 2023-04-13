using EdenNetwork;
using EdenNetwork.Udp;

namespace CSRServer.Game
{
    public class MaintainPhase
	{
		private const int INITIAL_TIME = 99;
		private readonly TurnData _turnData;
		private readonly GameManager _gameManager;
		private readonly EdenUdpServer _server;
        private readonly GameScene _parent;
        
        private readonly MaintainStore _maintainStore;

        private Timer? _timer;
		private int _time;
        private bool _turnEnd;

		public MaintainPhase(GameManager gameManager, EdenUdpServer server, TurnData turnData, GameScene parent)
		{
			this._gameManager = gameManager;
			this._server = server;
			this._turnData = turnData;
            this._parent = parent;
			
            this._time = 0;
            this._timer = null;
            _maintainStore = new MaintainStore(_turnData.playerList.Count);
        }

        /// <summary>
        /// 정비 페이즈 시작
        /// </summary>
        public void MaintainStart()
        {            
            _server.AddReceiveEvent("MaintainReady", MaintainReady);
            _server.AddResponse("RerollStore", RerollStore);
            _server.AddResponse("RerollRemoveCard", RerollRemoveCard);
            _server.AddResponse("BuyExp", BuyExp);
            _server.AddResponse("BuyCard", BuyCard);
            _server.AddResponse("RemoveCard", RemoveCard);
            
            _time = INITIAL_TIME;
            _turnEnd = false;

            //정비 페이즈 구성요소 클라이언트와 동기화
            foreach (var player in _turnData.playerList)
            {
                player.phaseReady = false;
                _maintainStore.ShowRandomCards(player, out var storeCards);
                _maintainStore.ShowRandomRemoveCards(player, out var removeCards);
                _server.Send("MaintainStart", player.clientId, new Dictionary<string, object>
                {
                    ["storeCards"] = storeCards,
                    ["removeCards"] = removeCards
                });
            }

            _timer = new Timer(GameTimer, null, 0, 1000);
        }

        /// <summary>
        /// 정비 페이즈 종료
        /// </summary>
        private void MaintainEnd()
        {
            lock (this)
            {
                if (_turnEnd == false)
                {
                    _timer?.Dispose();

                    _server.RemoveReceiveEvent("MaintainReady");
                    _server.RemoveResponse("RerollStore");
                    _server.RemoveResponse("RerollRemoveCard");
                    _server.RemoveResponse("BuyExp");
                    _server.RemoveResponse("BuyCard");
                    _server.RemoveResponse("RemoveCard");

                    _parent.PreheatStart();
                    _turnEnd = true;
                }
            }
        }
        
        // 정비 페이즈 타이머
        private async void GameTimer(object? sender)
        {
            if (_time >= 0)
            {
                _time--;
                await _server.BroadcastAsync("MaintainTime", _time, log: false);
            }
            else
            {
                MaintainEnd();
            }
        }

        #region Receive/Response Methods
        
        /// <summary>
        /// 정비 페이즈 준비완료 API
        /// </summary>
        private void MaintainReady(string clientId, EdenData data)
        {
            GamePlayer player = _turnData.playerMap[clientId];
            player.phaseReady = true;

            //모든 플레이어가 예열턴을 마쳤는지 체크
            bool checkAllReady = true;
            foreach (var p in _turnData.playerList)
                checkAllReady = checkAllReady && p.phaseReady;
            if (checkAllReady)
                MaintainEnd();
        }


        /// <summary>
        /// 구매카드 상점 리롤 API
        /// </summary>
        private EdenData RerollStore(string clientId, EdenData data)
        {
            GamePlayer player = _turnData.playerMap[clientId];
            if (player.phaseReady)
                return EdenData.Error("RerollStore - Player turn ends");
            
            if (_maintainStore.RerollStore(player, out var storeCards) == false)
                return EdenData.Error("RerollStore - Coin is not enough");

            return new EdenData(new Dictionary<string, object>
            {
                ["storeCards"] = storeCards!,
                ["coin"] = player.coin
            });

        }
        
        /// <summary>
        /// 제거카드 상점 리롤 API
        /// </summary>
        private EdenData RerollRemoveCard(string clientId, EdenData data)
        {

                GamePlayer player = _turnData.playerMap[clientId];
                if (player.phaseReady)
                    return EdenData.Error("RerollRemoveCard - Player turn ends");
                if (_maintainStore.RerollRemoveCard(player, out var removeCards) == false)
                    return EdenData.Error("RerollRemoveCard - Coin is not enough");
                
                return new EdenData(new Dictionary<string, object>
                {
                    ["removeCards"] = removeCards!,
                    ["coin"] = player.coin
                });
        }

        /// <summary>
        /// 경험치 구매 API
        /// </summary>
        private EdenData BuyExp(string clientId, EdenData data)
        {
            GamePlayer player = _turnData.playerMap[clientId];
            if (player.phaseReady)
                return EdenData.Error("BuyExp - Player turn ends");
            if (player.level == GamePlayer.MAX_LEVEL)
                return EdenData.Error("BuyExp - Player level is max");
            if (_maintainStore.BuyExp(player) == false)
                return EdenData.Error("BuyExp - Coin is not enough");

            return new EdenData(new Dictionary<string, object> {["level"] = player.level, ["exp"] = player.exp, ["expLimit"] = player.expLimit, ["coin"] = player.coin});
        }

        /// <summary>
        /// 구매카드 상점 구매 API
        /// </summary>
        private EdenData BuyCard(string clientId, EdenData data)
        {
            GamePlayer player = _turnData.playerMap[clientId];
                if (player.phaseReady)
                    return EdenData.Error("BuyCard - Player turn ends");
                if (player.coin < MaintainStore.COIN_BUY_CARD)
                    return EdenData.Error("BuyCard - Coin is not enough");
                if (!data.TryGet<int>(out var buyIndex))
                    return EdenData.Error("BuyCard - Buy index is missing");
                if (_maintainStore.BuyCard(player, buyIndex, out var storeCards, out var buyCard) == false)
                    return EdenData.Error($"BuyCard - Wrong store card index : {buyIndex}");

                player.AddCardToDeck(buyCard!);
                return new EdenData(new Dictionary<string, object>
                {
                    ["storeCards"] = storeCards,
                    ["buyCard"] = buyCard!,
                    ["deck"] = player.deck,
                    ["coin"] = player.coin
                });
        }

        /// <summary>
        /// 제커 카드 상점 제거 API
        /// </summary>
        private EdenData RemoveCard(string clientId, EdenData data)
        {
            GamePlayer player = _turnData.playerMap[clientId];
                if (player.phaseReady)
                    return EdenData.Error("RemoveCard - Player turn ends");
                if (player.coin < MaintainStore.COIN_REMOVE_CARD)
                    return EdenData.Error("BuyCard - Coin is not enough");
                if (!data.TryGet<int>(out var removeIndex))
                    return EdenData.Error("RemoveCard - remove index is missing");
                if (_maintainStore.RemoveCard(player, removeIndex, out var removeCards) == false)
                    return EdenData.Error($"RemoveCard - Cannot find remove card index {removeIndex}");

                return new EdenData(new Dictionary<string, object>
                {
                    ["removeCards"] = removeCards,
                    ["deck"] = player.deck,
                    ["coin"] = player.coin
                });
        }
        
        #endregion

    }
}