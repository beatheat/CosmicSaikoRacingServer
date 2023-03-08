using EdenNetwork;
using CSRServer.Lobby;
using CSRServer.Game;


namespace CSRServer
{
    internal class GameScene : Scene
    {
        #region Properties
        // 게임에 접속한 플레이어를 리스트와 맵 두가지로 관리함
        // clientId를 id값으로 playerMap에서 GamePlayer 검색하고 클라이언트에게는 playerList만 보내줌
        private readonly List<GamePlayer> playerList;
        private readonly Dictionary<string, GamePlayer> playerMap;

        private readonly List<Obstacle> obstacleList;
        
        private MaintainStore maintainStore;

        private Timer? gameTimer;
        // 전체 턴 수
        private int turn = 0;
        // 예열 섹션에서 시간제한을 알리는 변수
        private int time;
        //Phase진행순서 PREHEAT => DEPART -> MAINTAIN
        enum Phase
        {
            Preheat, Depart, Maintain
        }
        private Phase phase;

        #endregion
        #region Load Methods
        public GameScene(GameManager gameManager, EdenNetServer server) : base(gameManager, server)
        {
            playerList = new List<GamePlayer>();
            playerMap = new Dictionary<string, GamePlayer>();
            obstacleList = new List<Obstacle>();
            maintainStore = null!;
            gameTimer = null;
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
                GamePlayer gamePlayer = new GamePlayer(lbPlayer.clientId, playerList.Count, lbPlayer.nickname, playerList, obstacleList);
                playerList.Add(gamePlayer);
                playerMap.Add(gamePlayer.clientId, gamePlayer);
            }

            maintainStore = new MaintainStore(playerList.Count);
            
            server.AddReceiveEvent("PlayerReady", PlayerReady);

            server.AddResponse("UseCard", UseCard);
            server.AddResponse("RerollResource", RerollResource);

            server.AddResponse("RerollStore", RerollStore);
            server.AddResponse("RerollRemoveCard", RerollRemoveCard);
            server.AddResponse("BuyExp", BuyExp);
            server.AddResponse("BuyCard", BuyCard);
            server.AddResponse("RemoveCard", RemoveCard);
            
            server.AddReceiveEvent("PreheatEnd", PreheatEnd);
            server.AddReceiveEvent("DepartEnd", DepartEnd);
            server.AddReceiveEvent("MaintainEnd", MaintainEnd);

            
            server.BroadcastAsync("LobbyGameStart");
        }

        public override void Destroy()
        {
            gameTimer?.Dispose();
            server.RemoveReceiveEvent("PlayerReady");

            server.RemoveResponse("UseCard");
            server.RemoveResponse("RerollResource");
            
            server.RemoveResponse("RerollStore");
            server.RemoveResponse("RerollRemoveCard");
            server.RemoveResponse("BuyExp");
            server.RemoveResponse("BuyCard");
            server.RemoveResponse("RemoveCard");

            server.RemoveReceiveEvent("PreheatEnd");
            server.RemoveReceiveEvent("DepartEnd");
        }
        #endregion
        #region Game Methods

        private void PreheatStart()
        {
            time = 99;
            phase = Phase.Preheat;
            foreach (var player in playerList)
            {
                player.PreheatStart();
            }

            //TurnStart로 대체하자 -> playerList, playerIndex, turn
            var monitorPlayerList = GetMonitorPlayerList();
            foreach (var player in playerList)
            {
                server.SendAsync("PreheatStart", player.clientId, new Dictionary<string, object>
                {
                    ["player"] = player,
                    ["playerList"] = monitorPlayerList,
                    ["obstacleList"] = obstacleList,
                    ["turn"] = turn,
                    ["timer"] = time
                });
            }
        }

        private void DepartStart()
        {
            phase = Phase.Depart;
            time = 99;
            List<CardEffect.Result> attackResults = new List<CardEffect.Result>();
            List<Obstacle.Result> obstacleResults = new List<Obstacle.Result>();
            
            foreach (var player in playerList)
            {
                player.PreheatEnd(out var attackResult);
                attackResults.AddRange(attackResult);
                // obstacleResults.AddRange(obstacleResult);
                player.turnReady = false;
            }
            
            for (int i = obstacleList.Count - 1; i >= 0; i--)
            {
                if (obstacleList[i].Activate(out var obstacleResult))
                {
                    obstacleResults.Add(obstacleResult);
                    obstacleList.RemoveAt(i);
                }
            }

            server.BroadcastAsync("DepartStart", new Dictionary<string, object>
            {
                ["playerList"] = GetMonitorPlayerList(),
                ["attackResults"] = attackResults,
                ["obstacleResults"] = obstacleResults
            });
        }

        private void MaintainStart()
        {
            phase = Phase.Maintain;
            time = 99;

            //플레이어 rank 정해주기
            var orderedPlayerList = playerList.OrderBy(p => p.currentDistance).ToList();
            for (int i = 0; i < orderedPlayerList.Count; i++)
                orderedPlayerList[i].rank = i + 1;
            
            foreach (var player in playerList)
            {
                player.turnReady = false;
                maintainStore.ShowRandomCards(player, out var storeCards);
                maintainStore.ShowRandomRemoveCards(player, out var removeCards);
                server.SendAsync("MaintainStart", player.clientId, new Dictionary<string, object>
                {
                    ["storeCards"] = storeCards,
                    ["removeCards"] = removeCards
                });
            }
        }
        
        // 1초에 한번씩 실행함
        private void GameTimer(object? sender)
        {
            // if (time >= 0)
            // {
            //     time--;
            //     if(phase == Phase.Preheat)
            //         server.BroadcastAsync("PreheatTime", time);
            //     else if(phase == Phase.Maintain)
            //         server.BroadcastAsync("MaintainTime", time);
            // }
            // else
            // {
            //     if (phase == Phase.Preheat)
            //         DepartStart();
            //     else if (phase == Phase.Depart)
            //         MaintainStart();                    
            //     else if(phase == Phase.Maintain)
            //         PreheatStart();
            // }

        }
        
        private List<GamePlayer> GetMonitorPlayerList()
        {
            List<GamePlayer> monitorPlayerList = new List<GamePlayer>();
            foreach (var player in playerList)
            {
                monitorPlayerList.Add(player.CloneForMonitor());
            }
            return monitorPlayerList;
        }

        #endregion
        #region Receive/Response Methods
        
        
        private void PlayerReady(string clientId, EdenData data)
        {
            playerMap[clientId].turnReady = true;
            bool gameStart = true;
            foreach (var player in playerList)
            {
                gameStart = gameStart && player.turnReady;
            }

            if (gameStart)
            {
                turn = 1;
                PreheatStart();
                gameTimer = new Timer(GameTimer, null, 0, 1000);
            }
        }
        
        //예열턴
        private EdenData UseCard(string clientId, EdenData data)
        {
            if (phase == Phase.Preheat)
            {
                GamePlayer player = playerMap[clientId];
                if (player.turnReady)
                    return EdenData.Error("UseCard - Player turn ends");
                if (!data.TryGet<int>(out var useCardIndex))
                    return EdenData.Error("UseCard - Card index is missing");
                if(!player.IsCardEnable(useCardIndex))
                    return EdenData.Error("UseCard - Card does not satisfy resource condition");
                if (player.buffs[Buff.Type.Proliferation].count > 0)
                    return EdenData.Error("UseCard - Cannot use card caused by proliferation debuff");
                if(!player.UseCard(useCardIndex,out var result))
                    return new EdenData(new EdenError("UseCard - Card index is wrong"));
                return new EdenData(new Dictionary<string, object>
                {
                    ["player"] = player,
                    ["results"] = result
                });
            }
            return EdenData.Error("UseCard - Phase is not Preheat-Phase");
        }

        private EdenData RerollResource(string clientId, EdenData data)
        {
            if (phase == Phase.Preheat)
            {
                GamePlayer player = playerMap[clientId];
                if (player.turnReady)
                    return EdenData.Error("RollResource - Player turn ends");
                if (!data.TryGet<List<int>>(out var resourceFixed))
                    return EdenData.Error("RollResource - resourceFixed data is missing");
                var result = player.RollResource(resourceFixed);
                if (result == null)
                    return EdenData.Error("RollResource - Reroll Count is 0");
                return new EdenData(new Dictionary<string, object>
                {
                    ["resourceReel"] = result,
                    ["resourceRerollCount"] = player.resourceRerollCount
                });
            }
            return EdenData.Error("RollResource - Phase is not Preheat-Phase");
        }
        
        //정비턴
        private EdenData RerollStore(string clientId, EdenData data)
        {
            if (phase == Phase.Maintain)
            {
                GamePlayer player = playerMap[clientId];
                if (player.turnReady)
                    return EdenData.Error("RerollStore - Player turn ends");
                
                if (maintainStore.RerollStore(player, out var storeCards) == false)
                    return EdenData.Error("RerollStore - Coin is not enough");

                return new EdenData(new Dictionary<string, object>
                {
                    ["storeCards"] = storeCards!,
                    ["coin"] = player.coin
                });
            }
            return EdenData.Error("RerollStore - Phase is not Maintain-Phase");
        }
        private EdenData RerollRemoveCard(string clientId, EdenData data)
        {
            if (phase == Phase.Maintain)
            {
                GamePlayer player = playerMap[clientId];
                if (player.turnReady)
                    return EdenData.Error("RerollRemoveCard - Player turn ends");
                if (maintainStore.RerollRemoveCard(player, out var removeCards) == false)
                    return EdenData.Error("RerollRemoveCard - Coin is not enough");
                
                return new EdenData(new Dictionary<string, object>
                {
                    ["removeCards"] = removeCards!,
                    ["coin"] = player.coin
                });
            }
            return EdenData.Error("RerollRemoveCard - Phase is not Maintain-Phase");
        }

        private EdenData BuyExp(string clientId, EdenData data)
        {
            if (phase == Phase.Maintain)
            {
                GamePlayer player = playerMap[clientId];
                if (player.turnReady)
                    return EdenData.Error("BuyExp - Player turn ends");
                if (player.level == MaintainStore.MAX_LEVEL)
                    return EdenData.Error("BuyExp - Player level is max");
                if (maintainStore.BuyExp(player) == false)
                    return EdenData.Error("BuyExp - Coin is not enough");
                
                return new EdenData(new Dictionary<string, object>
                {
                    ["level"] = player.level,
                    ["exp"] = player.exp,
                    ["expLimit"] = player.expLimit,
                    ["coin"] = player.coin
                });
            }
            return EdenData.Error("BuyExp - Phase is not Maintain-Phase");
        }
        
        private EdenData BuyCard(string clientId, EdenData data)
        {
            if (phase == Phase.Maintain)
            {
                GamePlayer player = playerMap[clientId];
                if (player.turnReady)
                    return EdenData.Error("BuyCard - Player turn ends");
                if (player.coin < MaintainStore.COIN_BUY_CARD)
                    return EdenData.Error("BuyCard - Coin is not enough");
                if (!data.TryGet<int>(out var buyIndex))
                    return EdenData.Error("BuyCard - Buy index is missing");
                if (maintainStore.BuyCard(player, buyIndex, out var storeCards, out var buyCard) == false)
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
            return EdenData.Error("BuyCard - Phase is not Maintain-Phase");
        }

        private EdenData RemoveCard(string clientId, EdenData data)
        {
            if (phase == Phase.Maintain)
            {
                GamePlayer player = playerMap[clientId];
                if (player.turnReady)
                    return EdenData.Error("RemoveCard - Player turn ends");
                if (player.coin < MaintainStore.COIN_REMOVE_CARD)
                    return EdenData.Error("BuyCard - Coin is not enough");
                if (!data.TryGet<int>(out var removeIndex))
                    return EdenData.Error("RemoveCard - remove index is missing");
                if (maintainStore.RemoveCard(player, removeIndex, out var removeCards) == false)
                    return EdenData.Error($"RemoveCard - Cannot find remove card index {removeIndex}");

                return new EdenData(new Dictionary<string, object>
                {
                    ["removeCards"] = removeCards,
                    ["deck"] = player.deck,
                    ["coin"] = player.coin
                });            
            }
            return EdenData.Error("RemoveCard - Phase is not Maintain-Phase");
        }

        //==============

        private void PreheatEnd(string clientId, EdenData data)
        {
            if (phase == Phase.Preheat)
            {
                GamePlayer player = playerMap[clientId];
                player.turnReady = true;

                //모든 플레이어가 예열턴을 마쳤는지 체크
                bool checkAllReady = true;
                foreach (var p in playerList)
                    checkAllReady = checkAllReady && p.turnReady;
                if (checkAllReady)
                    DepartStart();
            }
        }
        
        private void DepartEnd(string clientId, EdenData data)
        {
            if (phase == Phase.Depart)
            {
                GamePlayer player = playerMap[clientId];
                player.turnReady = true;

                //모든 플레이어가 예열턴을 마쳤는지 체크
                bool checkAllReady = true;
                foreach (var p in playerList)
                    checkAllReady = checkAllReady && p.turnReady;
                if (checkAllReady)
                    MaintainStart();
            }
        }
        
        private void MaintainEnd(string clientId, EdenData data)
        {
            if (phase == Phase.Maintain)
            {
                GamePlayer player = playerMap[clientId];
                player.turnReady = true;

                //모든 플레이어가 예열턴을 마쳤는지 체크
                bool checkAllReady = true;
                foreach (var p in playerList)
                    checkAllReady = checkAllReady && p.turnReady;
                if (checkAllReady)
                    PreheatStart();
            }
        }



        #endregion
    }
}
