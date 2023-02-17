using System.Text.Json.Serialization;


namespace CSRServer.Game
{
    internal class GamePlayer
    {
        [JsonIgnore] public const int INITIAL_MAX_DISTANCE = 100;
        [JsonIgnore] public const int INITIAL_RESOURCE_REROLL_COUNT = 2;
        [JsonIgnore] public const int INITIAL_DRAW_COUNT = 5;
        
        [JsonIgnore]
        public string clientId;

        [JsonIgnore]
        private List<GamePlayer> parent;

        public int index;

        public string nickname;

        public int remainDistance;
        public int currentDistance;

        public int drawCount;

        [JsonIgnore]
        public bool turnReady;
        [JsonIgnore]
        public List<Card> turnUsedCard;

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
            
            deck = new List<Card>();
            hand = new List<Card>();
            usedCard = new List<Card>();
            unusedCard = new List<Card>();

            turnUsedCard = new List<Card>();

            resource = new List<ResourceType>();
            availableRerollCount = INITIAL_RESOURCE_REROLL_COUNT;
            resourceRerollCount = availableRerollCount;
            resourceCount = 5;
                
            artifact = new List<Artifact>();

            remainDistance = INITIAL_MAX_DISTANCE;
            currentDistance = 0;
            drawCount = INITIAL_DRAW_COUNT;

            coin = 10;
            exp = 0;
            level = 1;

            turnReady = false;
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
            return card.condition.Check(resource);
        }
        
        public bool UseCard(int index, out object[] result, out CardEffect.Type[] effectType)
        {
            if (index >= hand.Count || index < 0)
            {
                result = null!;
                effectType = null!;
                return false;
            }
            Card card = hand[index];
            //효과발동
            result = card.UseEffect(this);
            //효과 타입
            effectType = card.effect.GetTypes();
            //패에서 삭제
            hand.RemoveAt(index);
            //묘지로
            if (card.death)
                deck.Remove(card);
            else
                usedCard.Add(card);
            //타입 설정
            return true;
        }
        
        public List<ResourceType> RollResource(List<int>? resourceFixed)
        {
            if (resourceRerollCount > 0)
            {
                Random random = new Random();

                for (int i = 0; i < resourceCount; i++)
                {
                    ResourceType resourceElement = Util.GetRandomEnumValue<ResourceType>();
                    if (i >= resource.Count)
                    {
                        resource.Add(resourceElement);
                    }
                    else if (resourceFixed != null && !resourceFixed.Contains(i))
                    {
                        resource[i] = resourceElement;
                    }
                }
                resourceRerollCount--;
                return resource;
            }
            return new List<ResourceType>();
        }
        
        public List<Card> DrawCard(int count)
        {
            if (count < 0)
                return new List<Card>();
            //덱 회전에 관한 로직 짜기
            if (count >= unusedCard.Count)
                count = unusedCard.Count;
            List<Card> cards = new List<Card>(count);
            for (int i = 0; i < count; i++)
            {
                Random rand = new Random();
                int drawIndex = rand.Next(unusedCard.Count);
                hand.Add(unusedCard[drawIndex]);
                cards[i] = unusedCard[drawIndex];
                unusedCard.RemoveAt(drawIndex);
            }
            return cards;
        }

        public void AddCardToDeck(Card card)
        {
            deck.Add(card);
        }

        public bool RemoveCardFromDeck(int index)
        {
            if (index < 0 || index >= deck.Count)
                return false;
            deck.RemoveAt(index);
            return true;
        }
    }
}
