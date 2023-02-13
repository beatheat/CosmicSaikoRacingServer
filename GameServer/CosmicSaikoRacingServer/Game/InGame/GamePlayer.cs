using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CSRServer
{
    internal class GamePlayer
    {
        [JsonIgnore] public const int INITIAL_MAX_DISTANCE = 100;
        [JsonIgnore] public const int INITIAL_RESOURCE_REROLL_COUNT = 2;
        [JsonIgnore] public const int INITIAL_DRAW_COUNT = 5;
        
        [JsonIgnore]
        public string clientId;

        public int index;

        public string nickname;

        public int remainDistance;
        public int currentDistance;

        public int drawCount;

        [JsonIgnore]
        public bool turnReady;

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
        
        public GamePlayer(string clientId, int index, string nickname)
        {
            this.clientId = clientId;
            this.index = index;
            this.nickname = nickname;
            
            deck = new List<Card>();
            hand = new List<Card>();
            usedCard = new List<Card>();
            unusedCard = new List<Card>();

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

        public object[]? UseCard(int index, out List<CardEffectType>? effectType)
        {
            if (index >= hand.Count || index < 0)
            {
                effectType = null;
                return null;
            }
            
            effectType = hand[index].effectType;
            return hand[index].UseEffect(this);
        }
        
        public List<ResourceType> RollResource(List<int>? resourceFixed)
        {
            if (resourceRerollCount > 0)
            {
                Random random = new Random();

                for (int i = 0; i < resourceCount; i++)
                {
                    ResourceType _resource = (ResourceType) random.Next((int) ResourceType.All);
                    if (i >= resource.Count)
                    {
                        resource.Add(_resource);
                    }
                    else if (resourceFixed != null && !resourceFixed.Contains(i))
                    {
                        resource[i] = _resource;
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
