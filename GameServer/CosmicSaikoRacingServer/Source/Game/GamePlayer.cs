using System.Text.Json.Serialization;


namespace CSRServer.Game
{
    public class GamePlayer
    {
        //목표 이동거리
        [JsonIgnore] public const int INITIAL_TARGET_DISTANCE = 100;
        //최초 리롤 카운트
        [JsonIgnore] public const int INITIAL_RESOURCE_REROLL_COUNT = 4;
        //최초 카드 드로우 카운트
        [JsonIgnore] public const int INITIAL_DRAW_COUNT = 5;
        //최초 리소스릴 카운트
        [JsonIgnore] public const int INITIAL_RESOURCE_REEL_COUNT = 4;
        //최초 코인 카운트 , PreheatStart호출때 1을 더하기 떄문에 1적게 시작한다
        [JsonIgnore] public const int INITIAL_COIN_COUNT = 3; 
        //최대 코인 카운트
        [JsonIgnore] public const int MAX_COIN_COUNT = 10;
        //최대 레벨
        [JsonIgnore] public const int MAX_LEVEL = 5;

        
        //플레이어의 고유식별자
        [JsonIgnore]
        public string clientId  { private set; get; }
        //플레이어가 속한 리스트
        [JsonIgnore]
        public List<GamePlayer> parent { private set; get; }

        //리스트에서의 인덱스
        public int index { private set; get; }

        //플레이어의 닉네임
        public string nickname { private set; get; }

        //남은 이동거리
        public int remainDistance { private set; get; }
        //현재 이동거리
        public int currentDistance { private set; get; }
        //이번턴에 이동할 거리
        public int turnDistance { set; get; }
        
        //현재 등수 (이동거리가 크면 1등)
        public int rank { set; get; }

        //덱, 핸드, 묘지
        public List<Card> hand { private set; get; }
        public List<Card> deck { private set; get; }
        public List<Card> usedCard { private set; get; }
        public List<Card> unusedCard { private set; get; }


        //아티팩트 리스트
        public List<Artifact> artifactList { private set; get; }

        //버프 리스트
        [JsonIgnore]
        public BuffManager buffManager { private set; get; }
        public List<Buff> buffList { private set; get; }

        
        //페이즈 관련 데이터=======================================
        //한 페이즈에서의 레디여부
        [JsonIgnore]
        public bool phaseReady;
        
        //예열페이즈 관련 데이터-----------------------------------
        //이번턴에 드로우할 카드 수
        [JsonIgnore]
        public int drawCount { private set; get; }
        //이번턴에 사용한 카드 리스트
        [JsonIgnore]
        public List<Card> turnUsedCard { private set; get; }

        //리소스릴
        public List<Resource.Type> resourceReel;
        //리소스릴의 리롤 카운트
        public int resourceRerollCount;
        //턴 시작 시 부여받는 리롤 카운트
        [JsonIgnore]
        public int availableRerollCount;
        //리소스 릴 카운트
        [JsonIgnore]
        public int resourceReelCount;
        
        //발진페이즈 관련 데이터----------------------------------
        //발진페이즈에 실행할 이벤트 큐
        [JsonIgnore]
        private readonly Queue<Func<CardEffectModule.Result>> _departEvents;

        //정비페이즈 관련 데이터----------------------------------
        //코인
        public int coin;
        //경험치
        public int exp;
        //레벨별 경험치 한계치
        public int expLimit;
        //레벨
        public int level;
        //턴 당 시작 코인 수
        public int turnCoinCount;
        
        //페이즈
        [JsonIgnore]
        public GameScene scene;
        
        public GamePlayer(string clientId, int index, string nickname, List<GamePlayer> parent, GameScene scene)
        {
            this.clientId = clientId;
            this.index = index;
            this.nickname = nickname;

            this.parent = parent;
            this.scene = scene;

            rank = 1;
            deck = new List<Card>();
            hand = new List<Card>();
            usedCard = new List<Card>();
            unusedCard = new List<Card>();

            turnUsedCard = new List<Card>();

            resourceReel = new List<Resource.Type>();
            availableRerollCount = INITIAL_RESOURCE_REROLL_COUNT;
            resourceRerollCount = availableRerollCount;
            resourceReelCount = INITIAL_RESOURCE_REEL_COUNT;
            
            
            artifactList = new List<Artifact>();
            
            buffManager = new BuffManager(this);
            buffList = buffManager.buffList;

            remainDistance = INITIAL_TARGET_DISTANCE;
            currentDistance = 0;
            turnDistance = 0;
            
            drawCount = INITIAL_DRAW_COUNT;

            coin = INITIAL_COIN_COUNT;
            exp = 0;
            level = 1;
            expLimit = MaintainStore.expLimit[level];

            turnCoinCount = INITIAL_COIN_COUNT;

            phaseReady = false;
            
            
            _departEvents = new Queue<Func<CardEffectModule.Result>>();
            
            //임시 초기화
            int[,] card = {
                {0,0,1,1,3,3,4,4,6,6},
                {23,23,52,52,80,80,110,110,141,141},
                {21,21,25,25,26,26,30,30,32,32},
                {51,51,52,52,53,53,54,54,55,55},
                {52,52,53,53,54,54,56,56,57,57},
                // {80,80,84,84,85,85,87,87,92,92},
                {110,110,110,115,115,115,115,121,121,121},
                {140,140,144,144,146,146,157,157,151,151}
            };
            Random random = new Random();
            int randomNumber = random.Next(7);
            for (int i = 0; i < 10; i++)
            {
                AddCardToDeck(CardManager.GetCard(card[randomNumber,i]));
                // AddCardToDeck(CardManager.GetCard(140));
            }


            // for (int i = 160; i < 170; i++)
            // {
            //     AddCardToDeck(CardManager.GetCard(20));
            //     // AddCardToDeck(CardManager.GetCard(60));
            // }
        }

        /// <summary>
        /// 모니터링 플레이어 데이터는 덱정보 빼고 전달
        /// </summary>
        public GamePlayer CloneForMonitor()
        {
            GamePlayer hidePlayer = (GamePlayer)this.MemberwiseClone();
            hidePlayer.deck = null!;
            hidePlayer.usedCard = null!;
            hidePlayer.unusedCard = null!;
            return hidePlayer;
        }
        
        /// <summary>
        /// 카드가 리소스릴 조건에 맞는지 확인
        /// </summary>
        public bool IsCardEnable(int index)
        {
            if (index >= hand.Count || index < 0)
                return false;
            Card card = hand[index];
            return card.CheckCondition(resourceReel);
        }
        
        /// <summary>
        /// 패에 있는 카드 사용
        /// </summary>
        public bool UseCard(int index, out CardEffectModule.Result[] result)
        {
            if (index >= hand.Count || index < 0)
            {
                result = null!;
                return false;
            }
            Card card = hand[index];
            result = null!;
            //패에서 카드를 삭제
            hand.RemoveAt(index);
            // 카드 사용전 버프 적용
            if (buffManager.BeforeUseCard(ref card, ref result) == false)
            {
                //제거한 카드를 묘지로 보낸다
                MoveCardToGrave(card);
                return false;
            }
            //카드 사용
            result = card.UseEffect(this);
            //카드 사용가능 복구, 고장시 false로 바뀌어 복구한다.
            card.enable = true;
            //카드 사용후 버프 적용
            buffManager.AfterUseCard(ref card);
            //제거한 카드를 묘지로 보낸다
            MoveCardToGrave(card);
            //이번턴에 사용한 카드 리스트에 추가
            turnUsedCard.Add(card);
            return true;
        }
        
        /// <summary>
        /// 리소스릴에 있는 리소스를 랜덤으로 다시 뽑는다
        /// </summary>
        public List<Resource.Type> RollResource(List<int>? resourceFixed = null)
        {
            for (int i = 0; i < resourceReelCount; i++)
            {
                Resource.Type resource = Util.GetRandomEnumValue<Resource.Type>();
                //리소스릴 개수가 늘어났다면 리소스릴 새로 추가
                if (i >= resourceReel.Count)
                    resourceReel.Add(resource);
                //리소스릴 개수가 그대로라면 기존에 있던 릴에 덮어쓴다
                else if (resourceFixed == null || !resourceFixed.Contains(i))
                    resourceReel[i] = resource;
            }
            

            return resourceReel;
        }

        /// <summary>
        /// 최초로 받은 리소스를 이후 다시 배정받는다
        /// </summary>
        public List<Resource.Type>? RerollResource(List<int>? resourceFixed = null)
        {
            //리롤카운트가 0보다 클때만 사용할 수 있고 사용할때마다 1씩 줄어든다
            if (resourceRerollCount > 0)
            {
                resourceRerollCount--;
                //롤 리소스전 버프 적용
                buffManager.BeforeRerollResource(ref resourceFixed);

                RollResource(resourceFixed);
                //롤 리소스후 버프 적용
                buffManager.AfterRerollResource(ref resourceFixed, ref resourceReel);
                return resourceReel;
            }
            return null;
        }
        
        /// <summary>
        /// 덱에서 카드를 한장 뽑는다
        /// </summary>
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
                //덱에서 랜덤한 카드를 선택하여 패에 추가한다
                Random rand = new Random();
                int drawIndex = rand.Next(unusedCard.Count);
                Card card = unusedCard[drawIndex];
                //카드를 뽑았을 때 적용된 버프 제거
                card.isExposure = false;
                card.isMimesis = false; 
                
                hand.Add(unusedCard[drawIndex]);
                cards.Add(unusedCard[drawIndex]);
                unusedCard.RemoveAt(drawIndex);
                
                //카드를 뽑은후 버프 실행
                buffManager.OnDrawCard(ref card);
            }

            //드로우할 카드 수가 남은 덱의 수보다 많으면 묘지를 섞고 다시 드로우한다
            if (remainCount > 0)
            {
                //묘지와 덱을 스왑한다. 이때 덱은 항상 0장이다
                (usedCard, unusedCard) = (unusedCard, usedCard);
                if (remainCount > unusedCard.Count)
                    remainCount = unusedCard.Count;

                for (int i = 0; i < remainCount; i++)
                {
                    Random rand = new Random();
                    int drawIndex = rand.Next(unusedCard.Count);
                    Card card = unusedCard[drawIndex];
                    //카드를 뽑았을 때 적용된 버프 제거
                    card.isExposure = false;
                    card.isMimesis = false; 

                    hand.Add(unusedCard[drawIndex]);
                    cards.Add(unusedCard[drawIndex]);
                    unusedCard.RemoveAt(drawIndex);
                    //카드를 뽑은후 버프 실행
                    buffManager.OnDrawCard(ref card);
                }
            }
            
            return cards;
        }
        
        /// <summary>
        /// 카드를 패에 추가한다
        /// </summary>
        public void AddCardToHand(params Card[] card)
        {
            foreach (var c in card)
            {
                deck.Add(c);
                hand.Add(c);
            }
        }

        /// <summary>
        /// 카드를 덱에 추가한다
        /// </summary>
        public void AddCardToDeck(params Card[] card)
        {
            foreach (var c in card)
            {
                deck.Add(c);
                unusedCard.Add(c);
            }
        }

        /// <summary>
        /// 덱에서 카드를 제거한다
        /// </summary>
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

        /// <summary>
        /// 카드를 묘지로 옮긴다.
        /// </summary>
        public void MoveCardToGrave(Card card)
        {
            //제거한 카드를 묘지로 보낸다
            if (card.death)
            {
                deck.Remove(card);
                unusedCard.Remove(card);
                usedCard.Remove(card);
            }
            else
                usedCard.Add(card);
        }

        /// <summary>
        /// 패에서 카드를 제거한다
        /// </summary>
        public void DiscardCard(int index)
        {
            if (index >= hand.Count || index < 0)
            {
                return;
            }
            Card card = hand[index];

            //패에서 삭제
            hand.RemoveAt(index);
            //제거한 카드를 묘지로 보낸다
            MoveCardToGrave(card);
        }
        
        /// <summary>
        /// 패에서 카드제거와는 다른 "버리기"를 수행한다 
        /// </summary>
        public CardEffectModule.Result[] ThrowCard(int index)
        {
            if (index >= hand.Count || index < 0)
            {
                return Array.Empty<CardEffectModule.Result>();
            }
            Card card = hand[index];
            DiscardCard(index);
            //버리기 후 버프적용
            buffManager.OnThrowCard(card);
            return card.UseEffect(this, isDiscard: true);
        }

        /// <summary>
        /// 플레이어에게 버프를 추가한다
        /// </summary>
        public void AddBuff(Buff.Type type, int count)
        {
            buffManager.AddBuff(type, count);
        }
        

        //Turn Method==================
        /// <summary>
        /// 예열 페이즈가 실행할때 플레이어 정보를 설정한다
        /// </summary>
        public void PreheatStart()
        {
            phaseReady = false;
            resourceRerollCount = availableRerollCount;

            //코인 수 조절
            turnCoinCount++;
            if (turnCoinCount >= MAX_COIN_COUNT)
                turnCoinCount = MAX_COIN_COUNT;
            coin = turnCoinCount;
            
            RollResource();
            DrawCard(drawCount);
            
            buffManager.OnPreheatStart();
        }
        
        /// <summary>
        /// 예열 페이즈가 끝날때 플레이어 정보를 설정한다
        /// </summary>
        public void PreheatEnd()
        {
            //손패 전부 버리기
            while (hand.Count > 0)
            {
                DiscardCard(0);
            }

            //이번턴에 사용한 카드 리스트 초기화
            turnUsedCard.Clear();
            
            //버프 적용
            buffManager.OnPreheatEnd();
        }

        /// <summary>
        /// 발진 페이즈 시작시 공격 후 이동거리를 늘린다
        /// </summary>
        public void DepartStart(out CardEffectModule.Result[] attackResult)
        {
            //버프 적용
            buffManager.OnDepartStart();

            //발진 페이즈 시 발생할 공격정보를 보여준다
            CardEffectModule.Result[] _attackResult = new CardEffectModule.Result[_departEvents.Count];
            for (int idx = 0; _departEvents.Count > 0; idx++)
            {
                var turnEndEvent = _departEvents.Dequeue();
                _attackResult[idx] = turnEndEvent();
            }

            attackResult = _attackResult;

            currentDistance += turnDistance;
            remainDistance -= turnDistance;
            turnDistance = 0;
            
        }

        /// <summary>
        /// 발진 페이즈 발생할 이벤트 추가
        /// </summary>
        public void AddDepartEvent(Func<CardEffectModule.Result> departEvent)
        {
            _departEvents.Enqueue(departEvent);
        }
    }
}
