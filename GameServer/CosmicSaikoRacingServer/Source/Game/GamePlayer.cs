using System.Text.Json.Serialization;


namespace CSRServer.Game
{
    public class GamePlayer
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

        [JsonIgnore]
        public BuffManager buffManager { private set; get; }
        public List<Buff> buffList { private set; get; }
        
        //예열턴 관련 정보
        [JsonIgnore]
        public int drawCount { private set; get; }
        [JsonIgnore]
        public List<Card> turnUsedCard { private set; get; }
        [JsonIgnore]
        private readonly Queue<Func<CardEffect.Result>> _departEvents;
        
        
        //리소스 
        public List<Resource.Type> resourceReel;
        public int resourceRerollCount;
        [JsonIgnore]
        public int availableRerollCount;
        [JsonIgnore]
        public int resourceReelCount;
        

        //GameScene사용자료
        [JsonIgnore]
        public bool turnReady;
        public int rank;

        //정비턴 자료
        public int coin;
        public int exp;
        public int expLimit;
        public int level;

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
            
            buffManager = new BuffManager(this);
            buffList = buffManager.buffList;

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
            
            _departEvents = new Queue<Func<CardEffect.Result>>();
            
            //임시 초기화
            int[,] card = {
                {0,0,1,1,3,3,4,4,6,6},
                {23,23,52,52,80,80,110,110,141,141},
                {21,21,25,25,26,26,30,30,32,32}
            };
            Random random = new Random();
            int randomNumber = random.Next(3);
            for (int i = 0; i < 10; i++)
            {
                AddCardToDeck(CardManager.GetCard(card[randomNumber,i]));
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
        
        public bool UseCard(int index, out CardEffect.Result[] result)
        {
            if (index >= hand.Count || index < 0)
            {
                result = null!;
                return false;
            }
            Card card = hand[index];
            result = null!;

            // 카드 사용전 버프
            if (buffManager.BeforeUseCard(ref card) == false)
            {
                return false;
            }

            result = card.UseEffect(this);
            //카드 사용가능 복구, 고장시 false로 바뀌어 복구한다.
            card.enable = true;
            //효과 타입

            //카드 사용후 버프
            buffManager.AfterUseCard(ref card, ref result);
                
            DiscardCard(index);
            turnUsedCard.Add(card);
            return true;
        }
        
        
        public List<Resource.Type>? RollResourceInit(List<int>? resourceFixed = null)
        {
            buffManager.BeforeRollResource(ref resourceFixed);
            
            for (int i = 0; i < resourceReelCount; i++)
            {
                Resource.Type resource = Util.GetRandomEnumValue<Resource.Type>();
                if (i >= resourceReel.Count)
                    resourceReel.Add(resource);
                else if (resourceFixed != null && !resourceFixed.Contains(i))
                    resourceReel[i] = resource;
            }
            
            buffManager.AfterRollResource(ref resourceFixed, ref resourceReel);
            
            return resourceReel;
        }

        
        public List<Resource.Type>? RollResource(List<int>? resourceFixed = null)
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
                Card card = unusedCard[drawIndex];
                hand.Add(unusedCard[drawIndex]);
                cards.Add(unusedCard[drawIndex]);
                unusedCard.RemoveAt(drawIndex);
                
                buffManager.OnDrawCard(ref card);
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
                    Card card = unusedCard[drawIndex];

                    hand.Add(unusedCard[drawIndex]);
                    cards.Add(unusedCard[drawIndex]);
                    unusedCard.RemoveAt(drawIndex);

                    buffManager.OnDrawCard(ref card);
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
            buffManager.AddBuff(type, count);
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
            
            buffManager.OnTurnStart();
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
            
            //버프 적용
            buffManager.OnTurnEnd();

            //턴 종료시 미뤄둔 이벤트 전부 실행
            CardEffect.Result[] _attackResult = new CardEffect.Result[_departEvents.Count];
            for (int idx = 0; _departEvents.Count > 0; idx++)
            {
                var turnEndEvent = _departEvents.Dequeue();
                _attackResult[idx] = turnEndEvent();
            }

            attackResult = _attackResult;
            
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

        public void AddDepartEvent(Func<CardEffect.Result> departEvent)
        {
            _departEvents.Enqueue(departEvent);
        }
    }
}
