using System.Text.Json.Serialization;

namespace CSRServer.Game
{
    [JsonConverter(typeof(CardJsonConverter))]
    public class Card
    {
        /// <summary>
        /// 카드 타입
        /// </summary>
        public enum Type
        {
            Fossil, Electric, Bio, Nuclear, Cosmic, Normal
        }
        /// <summary>
        /// 카드 내장 변수
        /// </summary>
        public class Variable
        {
            public int value;
            public int lowerBound;
            public int upperBound;

            public Variable Clone() => (Variable)this.MemberwiseClone();
        }
        
        //카드의 식별자
        public int id;
        
        //카드 변수
        public Dictionary<string, Variable> variable;
        //카드 리소스 조건
        public CardCondition condition;
        
        //피폭 버프
        public bool isExposure = false;
        //의태 버프
        public bool isMimesis = false;
        //카드 소멸
        public bool death = false;
        
        //카드 타입
        [JsonIgnore]
        public Type type;
        //카드 등급
        [JsonIgnore] 
        public int rank;
        //카드 효과
        [JsonIgnore]
        public CardEffect effect;
        
        
        //카드효과가 발생할 것인지 여부
        [JsonIgnore]
        public bool enable = true;

        //카드 사용된 횟수
        [JsonIgnore] 
        public int usedCount = 0;

        public Card(int id, Type type, int rank, CardCondition condition, CardEffect effect, Dictionary<string,Variable> variable, bool death = false)
        {
            this.id = id;
            this.type = type;
            this.rank = rank;
            this.condition = condition;
            this.effect = effect;
            this.variable = variable;
            this.death = death;
        }
        
        public Card Clone()
        {
            var card = (Card)this.MemberwiseClone();
            var cloneVariables = new Dictionary<string, Variable>(card.variable.Count, card.variable.Comparer);
            foreach (var entry in card.variable)
            {
                cloneVariables.Add(entry.Key, entry.Value.Clone());
            }
            card.variable = cloneVariables;
            return card;
        }
        
        /// <summary>
        /// 카드 효과 사용, 버리기 시 버리기 효과 사용
        /// </summary>
        public CardEffectModule.Result[] UseEffect(GamePlayer gamePlayer, bool isDiscard = false)
        {
            usedCount++;
            var results = isDiscard ? effect.UseLeak(this, gamePlayer) : effect.Use(this, gamePlayer);
            return results;
        }
    }
}
