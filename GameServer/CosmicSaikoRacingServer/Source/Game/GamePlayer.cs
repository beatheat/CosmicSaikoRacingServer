using System.Text.Json.Serialization;


namespace CSRServer.Game
{
    internal class GamePlayer
    {
        [JsonIgnore] public const int INITIAL_MAX_DISTANCE = 100;
        [JsonIgnore] public const int INITIAL_RESOURCE_REROLL_COUNT = 2;
        [JsonIgnore] public const int INITIAL_DRAW_COUNT = 5;
        [JsonIgnore] public const int INITIAL_RESOURCE_COUNT = 5;
        [JsonIgnore] public const int INITIAL_COIN_COUNT = 10;
        
        [JsonIgnore]
        public string clientId;

        [JsonIgnore]
        public List<GamePlayer> parent;

        public int index;

        public string nickname;

        public int remainDistance;
        public int currentDistance;

        public int turnDistance;

        public int rank;
        
        public int drawCount;

        [JsonIgnore]
        public bool turnReady;
        [JsonIgnore]
        public List<Card> turnUsedCard;
        [JsonIgnore]
        private readonly Queue<EffectModule> _preheatTurnEndEvents;

        public List<Card> hand;
        public List<Card> deck;
        public List<Card> usedCard;
        public List<Card> unusedCard;

        public List<ResourceType> resource;
        public int resourceRerollCount;
        [JsonIgnore]
        public int availableRerollCount;
        [JsonIgnore]
        public int resourceCount;

        public List<Artifact> artifact;

        
        //정비턴 자료
        public int coin;
        public int exp;
        public int level;
        
        public GamePlayer(string clientId, int index, string nickname, List<GamePlayer> parent)
        {
            this.clientId = clientId;
            this.index = index;
            this.nickname = nickname;

            this.parent = parent;

            rank = 1;
            
            deck = new List<Card>();
            hand = new List<Card>();
            usedCard = new List<Card>();
            unusedCard = new List<Card>();

            turnUsedCard = new List<Card>();

            resource = new List<ResourceType>();
            availableRerollCount = INITIAL_RESOURCE_REROLL_COUNT;
            resourceRerollCount = availableRerollCount;
            resourceCount = INITIAL_RESOURCE_COUNT;
                
            artifact = new List<Artifact>();

            remainDistance = INITIAL_MAX_DISTANCE;
            currentDistance = 0;
            turnDistance = 0;
            
            drawCount = INITIAL_DRAW_COUNT;

            coin = INITIAL_COIN_COUNT;
            exp = 0;
            level = 1;

            turnReady = false;

            _preheatTurnEndEvents = new Queue<EffectModule>();
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
            return card.CheckCondition(resource);
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
            //효과발동
            result = card.UseEffect(this);
            //효과 타입
            ThrowCard(index);
            turnUsedCard.Add(card);
            return true;
        }
        
        
        public List<ResourceType>? RollResource(List<int>? resourceFixed = null)
        {
            if (resourceRerollCount > 0)
            {
                for (int i = 0; i < resourceCount; i++)
                {
                    ResourceType resourceElement = Util.GetRandomEnumValue<ResourceType>();
                    if (i >= resource.Count)
                        resource.Add(resourceElement);
                    else if (resourceFixed != null && !resourceFixed.Contains(i))
                        resource[i] = resourceElement;
                }
                resourceRerollCount--;
                return resource;
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

        public void AddCardToDeck(Card card)
        {
            deck.Add(card);
            usedCard.Add(card);
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
        
        public CardEffect.Result[] PreheatEnd()
        {
            //손패 전부 버리기
            while (hand.Count > 0)
            {
                ThrowCard(0);
            }

            //턴 종료시 미뤄둔 이벤트 전부 실행
            CardEffect.Result[] results = new CardEffect.Result[_preheatTurnEndEvents.Count];
            for (int idx = 0; _preheatTurnEndEvents.Count > 0; idx++)
            {
                var turnEndEvent = _preheatTurnEndEvents.Dequeue();
                results[idx] = turnEndEvent(null!, this, null!);
            }

            //이번턴에 실행한 카드 배열 초기화
            turnUsedCard.Clear();
            
            //턴 종료시 전진
            currentDistance += turnDistance;
            turnDistance = 0;
            
            return results;
        }

        public void AddPreheatEndEvent(EffectModule turnEndEvent)
        {
            _preheatTurnEndEvents.Enqueue(turnEndEvent);
        }
    }
}
