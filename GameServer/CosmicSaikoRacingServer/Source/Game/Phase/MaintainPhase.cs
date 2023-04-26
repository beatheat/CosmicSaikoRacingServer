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
        }

        /// <summary>
        /// 정비 페이즈 시작
        /// </summary>
        public void MaintainStart()
        {            
            _server.AddReceiveEvent("MaintainReady", MaintainReady);
            _server.AddResponse("RerollShop", RerollShop);
            _server.AddResponse("RerollRemoveCard", RerollRemoveCard);
            _server.AddResponse("BuyExp", BuyExp);
            _server.AddResponse("BuyCard", BuyCard);
            _server.AddResponse("RemoveCard", RemoveCard);
            
            _time = INITIAL_TIME;
            _turnEnd = false;
            //정비 페이즈 구성요소 클라이언트와 동기화
            foreach (var player in _turnData.playerList)
            {
                player.MaintainStart();
                _server.Send("MaintainStart", player.clientId, new Dictionary<string, object>
                {
                    ["shopCards"] = player.maintainSystem.shopCards,
                    ["removeCards"] = player.maintainSystem.removeCards,
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
                    _server.RemoveResponse("RerollShop");
                    _server.RemoveResponse("RerollRemoveCard");
                    _server.RemoveResponse("BuyExp");
                    _server.RemoveResponse("BuyCard");
                    _server.RemoveResponse("RemoveCard");
                    
                    foreach (var player in _turnData.playerList)
                        player.MaintainEnd();
                    
                    _parent.PreheatStart();
                    _turnEnd = true;
                }
            }
        }
        
        // 정비 페이즈 타이머
        private async void GameTimer(object? sender)
        {
            if (_time > 0)
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
        private EdenData RerollShop(string clientId, EdenData data)
        {
            GamePlayer player = _turnData.playerMap[clientId];
            if (player.phaseReady)
                return EdenData.Error("RerollShop - Player turn ends");

            var errorCode = player.maintainSystem.RerollShop();
            if (errorCode == MaintainSystem.ErrorCode.COIN_NOT_ENOUGH)
                return EdenData.Error("RerollShop - Coin is not enough");

            return new EdenData(new Dictionary<string, object>
            {
                ["shopCards"] = player.maintainSystem.shopCards,
                ["coin"] = player.maintainSystem.coin
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
                
                var errorCode = player.maintainSystem.RerollRemoveCard();
                if (errorCode == MaintainSystem.ErrorCode.COIN_NOT_ENOUGH)
                    return EdenData.Error("RerollRemoveCard - Coin is not enough");

                return new EdenData(new Dictionary<string, object>
                {
                    ["removeCards"] = player.maintainSystem.removeCards,
                    ["coin"] = player.maintainSystem.coin
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

            var errorCode = player.maintainSystem.BuyExp();
            
            if (errorCode == MaintainSystem.ErrorCode.COIN_NOT_ENOUGH)
                return EdenData.Error("BuyExp - Coin is not enough");
            else if (errorCode == MaintainSystem.ErrorCode.MAX_LEVEL)
                return EdenData.Error("BuyExp - Player level is max");

            return new EdenData(new Dictionary<string, object>
            {
                ["level"] = player.maintainSystem.level, 
                ["exp"] = player.maintainSystem.exp, 
                ["coin"] = player.maintainSystem.coin
            });
        }

        /// <summary>
        /// 구매카드 상점 구매 API
        /// </summary>
        private EdenData BuyCard(string clientId, EdenData data)
        {
            GamePlayer player = _turnData.playerMap[clientId];
            if (player.phaseReady)
                return EdenData.Error("BuyCard - Player turn ends");
            
            if (!data.TryGet<int>(out var buyIndex))
                return EdenData.Error("BuyCard - Buy index is missing");

            var errorCode = player.maintainSystem.BuyCard(buyIndex, out var boughtCard);
            if(errorCode == MaintainSystem.ErrorCode.COIN_NOT_ENOUGH)
                return EdenData.Error("BuyCard - Coin is not enough");
            else if(errorCode == MaintainSystem.ErrorCode.WRONG_INDEX)
                return EdenData.Error("BuyCard - Shop card index is wrong");
            

            player.cardSystem.AddCardToDeck(boughtCard);
            return new EdenData(new Dictionary<string, object>
            {
                ["shopCards"] = player.maintainSystem.shopCards,
                ["buyCard"] = boughtCard,
                ["deck"] = player.cardSystem.deck,
                ["coin"] = player.maintainSystem.coin
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
            
            if (!data.TryGet<int>(out var removeIndex))
                return EdenData.Error("RemoveCard - remove index is missing");
            
            var errorCode = player.maintainSystem.RemoveCard(removeIndex, out var removeCard);
            
            if (errorCode == MaintainSystem.ErrorCode.COIN_NOT_ENOUGH)
                return EdenData.Error("RemoveCard - Coin is not enough");
            
            if (errorCode == MaintainSystem.ErrorCode.WRONG_INDEX)
                return EdenData.Error($"RemoveCard - Remove card index is wrong");

            return new EdenData(new Dictionary<string, object>
            {
                ["removeCards"] = player.maintainSystem.removeCards,
                ["deck"] = player.cardSystem.deck,
                ["coin"] = player.maintainSystem.coin
            });
        }
        
        #endregion

    }
}