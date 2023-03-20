using EdenNetwork;

namespace CSRServer.Game
{

    internal class MaintainPhase
	{
		private const int INITIAL_TIME = 99;
		private readonly TurnData _turnData;
		private readonly GameManager _gameManager;
		private readonly EdenNetServer _server;
        private readonly GameScene _parent;
        
        private readonly MaintainStore _maintainStore;

        private Timer? _timer;
		private int _time;

		public MaintainPhase(GameManager gameManager, EdenNetServer server, TurnData turnData, GameScene parent)
		{
			this._gameManager = gameManager;
			this._server = server;
			this._turnData = turnData;
            this._parent = parent;
			
            this._time = 0;
            this._timer = null;
            _maintainStore = new MaintainStore(_turnData.playerList.Count);
        }

        public void MaintainStart()
        {
            _time = INITIAL_TIME;

            //플레이어 rank 정해주기
            var orderedPlayerList = _turnData.playerList.OrderBy(p => p.currentDistance).ToList();
            for (int i = 0; i < orderedPlayerList.Count; i++)
                orderedPlayerList[i].rank = i + 1;
            
            foreach (var player in _turnData.playerList)
            {
                player.turnReady = false;
                _maintainStore.ShowRandomCards(player, out var storeCards);
                _maintainStore.ShowRandomRemoveCards(player, out var removeCards);
                _server.SendAsync("MaintainStart", player.clientId, new Dictionary<string, object>
                {
                    ["storeCards"] = storeCards,
                    ["removeCards"] = removeCards
                });
            }

            _timer = new Timer(GameTimer, null, 0, 1000);
            
            _server.AddReceiveEvent("MaintainReady", MaintainReady);
            _server.AddResponse("RerollStore", RerollStore);
            _server.AddResponse("RerollRemoveCard", RerollRemoveCard);
            _server.AddResponse("BuyExp", BuyExp);
            _server.AddResponse("BuyCard", BuyCard);
            _server.AddResponse("RemoveCard", RemoveCard);
        }

        private void MaintainEnd()
        {
            _timer?.Dispose();
            
            _server.RemoveReceiveEvent("MaintainReady");
            _server.RemoveResponse("RerollStore");
            _server.RemoveResponse("RerollRemoveCard");
            _server.RemoveResponse("BuyExp");
            _server.RemoveResponse("BuyCard");
            _server.RemoveResponse("RemoveCard");        
            
            _parent.PreheatStart();
        }
        
        // 1초에 한번씩 실행함
        private void GameTimer(object? sender)
        {
            if (_time >= 0)
            {
                _time--;
                _server.BroadcastAsync("MaintainTime", _time);
            }
            else
            {
                MaintainEnd();
            }
        }

        private void MaintainReady(string clientId, EdenData data)
        {
            GamePlayer player = _turnData.playerMap[clientId];
            player.turnReady = true;

            //모든 플레이어가 예열턴을 마쳤는지 체크
            bool checkAllReady = true;
            foreach (var p in _turnData.playerList)
                checkAllReady = checkAllReady && p.turnReady;
            if (checkAllReady)
                MaintainEnd();
        }


        //정비턴
        private EdenData RerollStore(string clientId, EdenData data)
        {

            GamePlayer player = _turnData.playerMap[clientId];
            if (player.turnReady)
                return EdenData.Error("RerollStore - Player turn ends");
            
            if (_maintainStore.RerollStore(player, out var storeCards) == false)
                return EdenData.Error("RerollStore - Coin is not enough");

            return new EdenData(new Dictionary<string, object>
            {
                ["storeCards"] = storeCards!,
                ["coin"] = player.coin
            });

        }
        private EdenData RerollRemoveCard(string clientId, EdenData data)
        {

                GamePlayer player = _turnData.playerMap[clientId];
                if (player.turnReady)
                    return EdenData.Error("RerollRemoveCard - Player turn ends");
                if (_maintainStore.RerollRemoveCard(player, out var removeCards) == false)
                    return EdenData.Error("RerollRemoveCard - Coin is not enough");
                
                return new EdenData(new Dictionary<string, object>
                {
                    ["removeCards"] = removeCards!,
                    ["coin"] = player.coin
                });
        }

        private EdenData BuyExp(string clientId, EdenData data)
        {
            GamePlayer player = _turnData.playerMap[clientId];
            if (player.turnReady)
                return EdenData.Error("BuyExp - Player turn ends");
            if (player.level == MaintainStore.MAX_LEVEL)
                return EdenData.Error("BuyExp - Player level is max");
            if (_maintainStore.BuyExp(player) == false)
                return EdenData.Error("BuyExp - Coin is not enough");

            return new EdenData(new Dictionary<string, object> {["level"] = player.level, ["exp"] = player.exp, ["expLimit"] = player.expLimit, ["coin"] = player.coin});
        }

        private EdenData BuyCard(string clientId, EdenData data)
        {
            GamePlayer player = _turnData.playerMap[clientId];
                if (player.turnReady)
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

        private EdenData RemoveCard(string clientId, EdenData data)
        {
            GamePlayer player = _turnData.playerMap[clientId];
                if (player.turnReady)
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




    }
}