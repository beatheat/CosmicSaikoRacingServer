using System.Text.Json.Serialization;


namespace CSRServer.Game
{
    internal class GamePlayer
    {
        [JsonIgnore] public const int INITIAL_MAX_DISTANCE = 100;
        [JsonIgnore] public const int INITIAL_RESOURCE_REROLL_COUNT = 100;
        [JsonIgnore] public const int INITIAL_DRAW_COUNT = 5;
        [JsonIgnore] public const int INITIAL_RESOURCE_COUNT = 4;
        [JsonIgnore] public const int INITIAL_COIN_COUNT = 3; //PreheatStart호출 떄문에 1적게 시작한다
        [JsonIgnore] public const int MAX_COIN_COUNT = 10;
        
        [JsonIgnore]
        public string clientId  { private set; get; }
        [JsonIgnore]
        public List<GamePlayer> parent { private set; get; }
        [JsonIgnore]
        public List<Obstacle> obstacleList { private set; get; }

        public int index { private set; get; }

        public string nickname { private set; get; }

        //게임 목표 
        public int remainDistance { private set; get; }
        public int currentDistance { private set; get; }
        //EffectModule에서 사용
        public int turnDistance { set; get; }

        //덱, 핸드, 묘지
        public List<Card> hand { private set; get; }
        public List<Card> deck { private set; get; }
        public List<Card> usedCard { private set; get; }
        public List<Card> unusedCard { private set; get; }


        public List<Artifact> artifactList { private set; get; }

        public Dictionary<Buff.Type, Buff> buffs { private set; get; }

        //예열턴 관련 정보
        [JsonIgnore]
        public int drawCount { private set; get; }
        [JsonIgnore]
        public List<Card> turnUsedCard { private set; get; }
        [JsonIgnore]
        private readonly Queue<Func<CardEffect.Result>> preheatTurnEndEvents;
        
        
        //리소스 
        public List<ResourceType> resourceReel { private set; get; }
        public int resourceRerollCount{ set; get; }
        [JsonIgnore]
        public int availableRerollCount { set; get; }
        [JsonIgnore]
        public int resourceReelCount { set; get; }
        

        //GameScene사용자료
        [JsonIgnore]
        public bool turnReady { set; get; }
        public int rank { set; get; }

        //정비턴 자료
        public int coin { set; get; }
        public int exp { set; get; }
        public int expLimit { set; get; }
        public int level { set; get; }

        private int turnCoinCount;
        
        public GamePlayer(string clientId, int index, string nickname, List<GamePlayer> parent, List<Obstacle> obstacleList)
        {
            this.clientId = clientId;
            this.index = index;
            this.nickname = nickname;

            this.parent = parent;
            this.obstacleList = obstacleList;

            rank = 1;
            deck = new List<Card>();
            hand = new List<Card>();
            usedCard = new List<Card>();
            unusedCard = new List<Card>();

            turnUsedCard = new List<Card>();

            resourceReel = new List<ResourceType>();
            availableRerollCount = INITIAL_RESOURCE_REROLL_COUNT;
            resourceRerollCount = availableRerollCount;
            resourceReelCount = INITIAL_RESOURCE_COUNT;
            
            
            artifactList = new List<Artifact>();
            buffs = BuffManager.CreateBuffDictionary();

            remainDistance = INITIAL_MAX_DISTANCE;
            currentDistance = 0;
            turnDistance = 0;
            
            drawCount = INITIAL_DRAW_COUNT;

            coin = INITIAL_COIN_COUNT;
            exp = 0;
            level = 1;
            expLimit = MaintainStore.expLimit[level];

            turnCoinCount = INITIAL_COIN_COUNT;

            turnReady = false;
            
            preheatTurnEndEvents = new Queue<Func<CardEffect.Result>>();

            //임시 초기화
            for (int i = 0; i < 10; i++)
            {
                Card card = CardManager.GetCard(i);
                AddCardToDeck(card);
            }
        }

        public GamePlayer CloneForMonitor()
        {
            GamePlayer hidePlayer = (GamePlayer)this.MemberwiseClone();
            hidePlayer.deck = null!;
            hidePlayer.usedCard = null!;
            hidePlayer.unusedCard = null!;
            return hidePlayer;
        }

        public bool IsCardEnable(int index)
        {
            if (index >= hand.Count || index < 0)
                return false;
            Card card = hand[index];
            return card.CheckCondition(resourceReel);
        }
        
        public bool UseCard(int index, out List<CardEffect.Result> result)
        {
            if (index >= hand.Count || index < 0)
            {
                result = null!;
                return false;
            }
            Card card = hand[index];
            result = null!;
            //고장(BREAK_DOWN)버프
            if (buffs[Buff.Type.BreakDown].count > 0)
            {
                Random random = new Random();
                if (random.Next(2) != 0)
                    card.enable = false;
                buffs[Buff.Type.BreakDown].count--;
            }
            //효과발동
            result = card.UseEffect(this);
            card.enable = true;
            //효과 타입
            ThrowCard(index);
            turnUsedCard.Add(card);
            return true;
        }
        
        
        public List<ResourceType>? RollResourceInit(List<int>? resourceFixed = null)
        {
            //누전(ELECTRIC_LEAK)버프가 있으면 적용한다
            resourceFixed?.AddRange(buffs[Buff.Type.ElectricLeak].GetVariable<List<int>>("resourceLockIndexList")!);
            
            for (int i = 0; i < resourceReelCount; i++)
            {
                ResourceType resource = Util.GetRandomEnumValue<ResourceType>();
                if (i >= resourceReel.Count)
                    resourceReel.Add(resource);
                else if (resourceFixed != null && !resourceFixed.Contains(i))
                    resourceReel[i] = resource;
            }
            return resourceReel;
        }

        
        public List<ResourceType>? RollResource(List<int>? resourceFixed = null)
        {
            if (resourceRerollCount > 0)
            {
                resourceRerollCount--;
                return RollResourceInit(resourceFixed);
            }
            return null;
        }
        
        public List<Card>? DrawCard(int count)
        {
            if (count < 0)
                return null;
            int remainCount = 0;
            
            List<Card> cards = new List<Card>();
            
            if (count > unusedCard.Count)
            {
                remainCount = count - unusedCard.Count;
                count = unusedCard.Count;
            }
            
            for (int i = 0; i < count; i++)
            {
                Random rand = new Random();
                int drawIndex = rand.Next(unusedCard.Count);
                hand.Add(unusedCard[drawIndex]);
                cards.Add(unusedCard[drawIndex]);
                unusedCard.RemoveAt(drawIndex);
            }

            if (remainCount > 0)
            {
                (usedCard, unusedCard) = (unusedCard, usedCard);
                if (remainCount > unusedCard.Count)
                    remainCount = unusedCard.Count;

                for (int i = 0; i < remainCount; i++)
                {
                    Random rand = new Random();
                    int drawIndex = rand.Next(unusedCard.Count);
                    hand.Add(unusedCard[drawIndex]);
                    cards.Add(unusedCard[drawIndex]);
                    unusedCard.RemoveAt(drawIndex);
                }
            }
            
            return cards;
        }
        
        public void AddCardToHand(params Card[] card)
        {
            foreach (var c in card)
            {
                deck.Add(c);
                hand.Add(c);
            }
        }

        public void AddCardToDeck(params Card[] card)
        {
            foreach (var c in card)
            {
                deck.Add(c);
                usedCard.Add(c);
            }
        }

        public bool RemoveCardFromDeck(int index)
        {
            if (index < 0 || index >= deck.Count)
                return false;
            Card card = deck[index];
            deck.RemoveAt(index);
            unusedCard.Remove(card);
            usedCard.Remove(card);
            return true;
        }

        public void ThrowCard(int index)
        {
            if (index >= hand.Count || index < 0)
            {
                return;
            }
            Card card = hand[index];
            //패에서 삭제
            hand.RemoveAt(index);
            //묘지로
            if (card.death)
            {
                deck.Remove(card);
                unusedCard.Remove(card);
                usedCard.Remove(card);
            }
            else
                usedCard.Add(card);
        }

        public void AddBuff(Buff.Type type, int count)
        {
            buffs[type].count += count;
        }
        

        public void PreheatStart()
        {
            turnReady = false;
            resourceRerollCount = availableRerollCount;

            //코인 수 조절
            turnCoinCount++;
            if (turnCoinCount >= MAX_COIN_COUNT)
                turnCoinCount = MAX_COIN_COUNT;
            coin = turnCoinCount;
            
            RollResourceInit();
            DrawCard(drawCount);
            
            foreach (var buff in buffs.Values)
            {
                buff.Init(this);
            }
        }
        
        public void PreheatEnd(out CardEffect.Result[] attackResult)
        {
            //손패 전부 버리기
            while (hand.Count > 0)
            {
                ThrowCard(0);
            }

            //이번턴에 실행한 카드 배열 초기화
            turnUsedCard.Clear();
            
            //버프 릴리즈
            foreach (var buff in buffs.Values)
            {
                buff.Release();
            }

            //턴 종료시 미뤄둔 이벤트 전부 실행
            CardEffect.Result[] _attakResult = new CardEffect.Result[preheatTurnEndEvents.Count];
            for (int idx = 0; preheatTurnEndEvents.Count > 0; idx++)
            {
                var turnEndEvent = preheatTurnEndEvents.Dequeue();
                _attakResult[idx] = turnEndEvent();
            }

            attackResult = _attakResult;

            //턴 종료시 전진 + 고효율&저효율 버프 적용
            turnDistance = (int) (turnDistance * (1.0 + 0.1 * (buffs[Buff.Type.HighEfficiency].count - buffs[Buff.Type.LowEfficiency].count)));

            //방해물 밟기
            foreach (var obstacle in obstacleList)
            {
                //방해물 밟음
                if (obstacle.location > currentDistance && obstacle.location < currentDistance + turnDistance)
                {
                    obstacle.SetActivatePlayer(this);
                }
            }
            
            currentDistance += turnDistance;
            remainDistance -= turnDistance;
            turnDistance = 0;
            
        }

        public void AddPreheatEndEvent(Func<CardEffect.Result> turnEndEvent)
        {
            preheatTurnEndEvents.Enqueue(turnEndEvent);
        }
    }
}
