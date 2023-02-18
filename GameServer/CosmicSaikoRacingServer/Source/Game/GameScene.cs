﻿using EdenNetwork;
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

        private MaintainStore maintainStore;

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
            maintainStore = null!;
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
                GamePlayer gamePlayer = new GamePlayer(lbPlayer.clientId, playerList.Count, lbPlayer.nickname, playerList);
                playerList.Add(gamePlayer);
                playerMap.Add(gamePlayer.clientId, gamePlayer);
            }

            maintainStore = new MaintainStore(playerList.Count);
            
            server.AddResponse("UseCard", UseCard);
            server.AddResponse("RollResource", RollResource);

            server.AddResponse("RerollStore", RerollStore);
            server.AddResponse("BuyExp", BuyExp);
            server.AddResponse("BuyCard", BuyCard);
            server.AddResponse("RemoveCard", RemoveCard);
            
            server.AddReceiveEvent("PreheatEnd", PreheatEnd);
            server.AddReceiveEvent("DepartEnd", DepartEnd);

            turn = 1;
            PreheatStart();
            Timer timer = new Timer(GameTimer, null, 0, 1000);
        }

        public override void Destroy()
        {
            server.RemoveResponse("UseCard");
            server.RemoveResponse("RollResource");
            
            server.RemoveResponse("RerollStore");
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
                player.RollResource(null);
                player.DrawCard(player.drawCount);
                player.turnReady = false;
                player.resourceRerollCount = player.availableRerollCount;
            }

            //TurnStart로 대체하자 -> playerList, playerIndex, turn
            var monitorPlayerList = GetMonitorPlayerList();
            foreach (var player in playerList)
            {
                server.SendAsync("PreheatStart", player.clientId, new Dictionary<string, object>
                {
                    ["player"] = player,
                    ["playerList"] = monitorPlayerList,
                    ["turn"] = turn,
                    ["timer"] = time
                });
            }
        }

        private void DepartStart()
        {
            phase = Phase.Depart;
            time = 99;
            foreach (var player in playerList)
                player.turnReady = false;
            server.BroadcastAsync("DepartStart", GetMonitorPlayerList());
        }

        private void MaintainStart()
        {
            phase = Phase.Maintain;
            time = 99;
            foreach (var player in playerList)
            {
                player.turnReady = false;
                var storeCards = maintainStore.ShowRandomCards(player.index, player.level);
                server.SendAsync("MaintainStart", player.clientId, storeCards);
            }
        }
        
        // 1초에 한번씩 실행함
        private void GameTimer(object? sender)
        {
            if (time >= 0)
            {
                time--;
                if(phase == Phase.Preheat)
                    server.BroadcastAsync("PreheatTime", time);
                else if(phase == Phase.Maintain)
                    server.BroadcastAsync("MaintainTime", time);
            }
            else
            {
                if (phase == Phase.Preheat)
                    DepartStart();
                else if (phase == Phase.Depart)
                    MaintainStart();                    
                else if(phase == Phase.Maintain)
                    PreheatStart();
            }

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
        
        //예열턴
        private EdenData UseCard(string clientId, EdenData data)
        {
            if (phase == Phase.Preheat)
            {
                if (!data.TryGet<int>(out var useCardIndex))
                    return new EdenData(new EdenError("UseCard - Card index is missing"));
                GamePlayer player = playerMap[clientId];
                if(!player.IsCardEnable(useCardIndex))
                    return new EdenData(new EdenError("UseCard - Card does not satisfy resource condition"));
                if(!player.UseCard(useCardIndex,out var result, out var effectType))
                    return new EdenData(new EdenError("UseCard - Card index is wrong"));
                var response = new Dictionary<string, object> {["effectType"] = effectType, ["results"] = result};
                return new EdenData(response);
            }
            return new EdenData(new EdenError("UseCard - Phase is not Preheat-Phase"));
        }

        private EdenData RollResource(string clientId, EdenData data)
        {
            if (phase == Phase.Preheat)
            {
                if (!data.TryGet<List<int>>(out var resourceFixed))
                    return new EdenData(new EdenError("RollResource - resourceFixed data is missing"));
                GamePlayer player = playerMap[clientId];
                var result = player.RollResource(resourceFixed);
                if (result.Count == 0)
                    return new EdenData(new EdenError("RollResource - Reroll Count is 0"));
                return new EdenData(result);
            }
            return new EdenData(new EdenError("RollResource - Phase is not Preheat-Phase"));
        }
        
        //정비턴
        private EdenData RerollStore(string clientId, EdenData data)
        {
            if (phase == Phase.Maintain)
            {
                GamePlayer player = playerMap[clientId];
                var storeCards = maintainStore.ShowRandomCards(player.index, player.level);
                return new EdenData(storeCards);
            }
            return new EdenData(new EdenError("RerollStore - Phase is not Maintain-Phase"));
        }

        private EdenData BuyExp(string clientId, EdenData data)
        {
            if (phase == Phase.Maintain)
            {
                //구현필요
            }
            return new EdenData(new EdenError("BuyExp - Phase is not Maintain-Phase"));
        }
        
        private EdenData BuyCard(string clientId, EdenData data)
        {
            if (phase == Phase.Maintain)
            {
                GamePlayer player = playerMap[clientId];
                if (!data.TryGet<int>(out var buyIndex))
                    return new EdenData(new EdenError("BuyCard - Cannot find store card index"));
                Card? buyCard = maintainStore.BuyCard(player.index, buyIndex);
                if (buyCard == null)
                {
                    return new EdenData(new EdenError("BuyCard - Store card index is wrong"));
                }
                player.AddCardToDeck(buyCard);
                return new EdenData(buyCard);
            }
            return new EdenData(new EdenError("BuyCard - Phase is not Maintain-Phase"));
        }

        private EdenData RemoveCard(string clientId, EdenData data)
        {
            if (phase == Phase.Maintain)
            {
                GamePlayer player = playerMap[clientId];
                if (!data.TryGet<int>(out var removeIndex))
                    return new EdenData(new EdenError("RemoveCard - remove index is missing"));
                bool success = player.RemoveCardFromDeck(removeIndex);
                if (success == false)
                    return new EdenData(new EdenError("RemoveCard - Remove index is wrong"));
                return new EdenData(player.deck);
            }
            return new EdenData(new EdenError("RemoveCard - Phase is not Maintain-Phase"));
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


        #endregion
    }
}