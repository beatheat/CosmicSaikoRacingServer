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
        private readonly Queue<Func<CardEffect.Result>> _preheatTurnEndEvents;
        
        
        //리소스 
        public List<Resource.Type> resourceReel { private set; get; }
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

        private int _turnCoinCount;
        
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

            resourceReel = new List<Resource.Type>();
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

            _turnCoinCount = INITIAL_COIN_COUNT;

            turnReady = false;
            
            _preheatTurnEndEvents = new Queue<Func<CardEffect.Result>>();
            
            //임시 초기화
            for (int i = 0; i < 10; i++)
            {
                Card card = CardManager.GetCard(i);
                AddCardToDeck(card);
            }
            AddCardToDeck(CardManager.GetCard(50));
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
        
        public bool UseCard(int index, out CardEffect.Result[] result)
        {
            if (index >= hand.Count || index < 0)
            {
                result = null!;
                return false;
            }
            Card card = hand[index];
            result = null!;
            //증식 버프로 인해 카드 사용불가
            if (buffs[Buff.Type.Proliferation].count > 0)
                return false;
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
            if (buffs[Buff.Type.HighDensity].count > 0)
            {
                result = result.Concat(card.UseEffect(this)).ToArray();
                buffs[Buff.Type.HighDensity].count--;
            }
            card.enable = true;
            //효과 타입
            DiscardCard(index);
            turnUsedCard.Add(card);
            return true;
        }
        
        
        public List<Resource.Type>? RollResourceInit(List<int>? resourceFixed = null)
        {
            //누전(ELECTRIC_LEAK)버프가 있으면 적용한다
            if(buffs[Buff.Type.ElectricLeak].count > 0)
                resourceFixed?.AddRange(buffs[Buff.Type.ElectricLeak].GetVariable<List<int>>("resourceLockIndexList")!);
            
            for (int i = 0; i < resourceReelCount; i++)
            {
                Resource.Type resource = Util.GetRandomEnumValue<Resource.Type>();
                if (i >= resourceReel.Count)
                    resourceReel.Add(resource);
                else if (resourceFixed != null && !resourceFixed.Contains(i))
                    resourceReel[i] = resource;
            }
            return resourceReel;
        }

        
        public List<Resource.Type>? RollResource(List<int>? resourceFixed = null)
        {
            if (resourceRerollCount > 0)
            {
                resourceRerollCount--;
                //증식버프가 해제되었는지 체크
                if (buffs[Buff.Type.Proliferation].count > 0 && resourceReel.Contains(buffs[Buff.Type.Proliferation].GetVariable<List<Resource.Type>>("resourceCondition")))
                {
                    buffs[Buff.Type.Proliferation].count = 0;
                }
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
                
                //새로 뽑는 카드에 피폭적용
                if(buffs[Buff.Type.Exposure].count > 0)
                    buffs[Buff.Type.Exposure].Apply(this);
            }

            //드로우 수가 남은 덱의 수보다 많으면 묘지를 섞고 다시 드로우
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

                    //새로 뽑는 카드에 피폭적용
                    if(buffs[Buff.Type.Exposure].count > 0)
                        buffs[Buff.Type.Exposure].Apply(this);
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

        public void DiscardCard(int index)
        {
            if (index >= hand.Count || index < 0)
            {
                return;
            }
            Card card = hand[index];
            card.isExposure = false;
            card.isMimesis = false;
            card.condition = null;
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

        public CardEffect.Result[] ThrowCard(int index)
        {
            if (index >= hand.Count || index < 0)
            {
                return Array.Empty<CardEffect.Result>();
            }
            Card card = hand[index];
            DiscardCard(index);
            return card.UseEffect(this, isDiscard: true);
        }

        public void AddBuff(Buff.Type type, int count)
        {
            buffs[type].count += count;
        }
        

        //Turn Method==================
        public void PreheatStart()
        {
            turnReady = false;
            resourceRerollCount = availableRerollCount;

            //코인 수 조절
            _turnCoinCount++;
            if (_turnCoinCount >= MAX_COIN_COUNT)
                _turnCoinCount = MAX_COIN_COUNT;
            coin = _turnCoinCount;
            
            RollResourceInit();
            DrawCard(drawCount);
            
            foreach (var buff in buffs.Values)
            {
                buff.Apply(this);
            }

            // 증식 버프가 해제되었는지 체크
            if (buffs[Buff.Type.Proliferation].count > 0 && resourceReel.Contains(buffs[Buff.Type.Proliferation].GetVariable<List<Resource.Type>>("resourceCondition")))
            {
                buffs[Buff.Type.Proliferation].count = 0;
            }
        }
        
        public void PreheatEnd(out CardEffect.Result[] attackResult)
        {
            //손패 전부 버리기
            while (hand.Count > 0)
            {
                DiscardCard(0);
            }

            //이번턴에 실행한 카드 배열 초기화
            turnUsedCard.Clear();
            
            //버프 릴리즈
            foreach (var buff in buffs.Values)
            {
                buff.Release();
            }

            //턴 종료시 미뤄둔 이벤트 전부 실행
            CardEffect.Result[] _attackResult = new CardEffect.Result[_preheatTurnEndEvents.Count];
            for (int idx = 0; _preheatTurnEndEvents.Count > 0; idx++)
            {
                var turnEndEvent = _preheatTurnEndEvents.Dequeue();
                _attackResult[idx] = turnEndEvent();
            }

            attackResult = _attackResult;

            //턴 종료시 전진 + 고효율 버프 적용
            turnDistance = (int) (turnDistance * (1.0 + 0.1 * (buffs[Buff.Type.HighEfficiency].count)));

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
            _preheatTurnEndEvents.Enqueue(turnEndEvent);
        }
    }
}
