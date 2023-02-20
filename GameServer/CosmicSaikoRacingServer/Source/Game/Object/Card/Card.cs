using System.Text.Json.Serialization;

namespace CSRServer.Game
{
    internal class Card
    {
        public enum Type
        {
            Fossil, Electric, Bio, Nuclear, Cosmic, Normal
        }
        
        //카드 기본정보
        public int id;
        public Dictionary<string, int> variable;

        [JsonIgnore]
        public Type type;
        [JsonIgnore] 
        public int rank;
        [JsonIgnore]
        public CardEffect effect;
        [JsonIgnore]
        public CardCondition condition;
        
        //카드 자체생성정보

        [JsonIgnore]
        public bool enable = true;
        [JsonIgnore] 
        public bool death = false;
        [JsonIgnore] 
        public int percentage = 0;
        [JsonIgnore] 
        public int usedCount = 0;
        [JsonIgnore] 
        public int useCountAtOnes = 1;

        public Card(int id, Type type, int rank, CardCondition condition, CardEffect effect)
        {
            this.id = id;
            this.type = type;
            this.rank = rank;
            this.condition = condition;
            this.effect = effect;
        }
        
        public Card Clone()
        {
            return (Card)this.MemberwiseClone();
        }
        
        public List<CardEffect.Result> UseEffect(GamePlayer gamePlayer)
        {
            List<CardEffect.Result> results = new List<CardEffect.Result>();
            for (int i = 0; i < useCountAtOnes; i++)
            {
                results.AddRange(effect.Use(this, gamePlayer));
                usedCount++;
            }
            useCountAtOnes = 1;
            return results;
        }

        public bool CheckCondition(List<ResourceType> resource)
        {
            return condition.Check(resource);
        }
        
    }
}
