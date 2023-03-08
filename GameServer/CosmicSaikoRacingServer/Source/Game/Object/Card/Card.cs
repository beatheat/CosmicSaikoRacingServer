using System.Text.Json.Serialization;

namespace CSRServer.Game
{
    internal class Card
    {
        public enum Type
        {
            Fossil, Electric, Bio, Nuclear, Cosmic, Normal
        }

        public class Variable
        {
            public int value;
            public int lowerBound;
            public int upperBound;
        }
        
        //카드 기본정보
        public int id;
        
        //실제사용객체
        [JsonIgnore]
        public Dictionary<string, Variable> _variable;
        //데이터 최소화를 위한 전송 객체
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, Variable>? variable;

        //실제 사용 객체
        [JsonIgnore]
        public CardCondition _condition;
        //데이터 최소화를 위한 전송 객체
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CardCondition? condition;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool isExposure = false;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool isMimesis = false;

        [JsonIgnore]
        public Type type;
        [JsonIgnore] 
        public int rank;
        [JsonIgnore]
        public CardEffect effect;
        
        
        //카드 자체생성정보

        [JsonIgnore]
        public bool enable = true;
        [JsonIgnore] 
        public bool death = false;
        [JsonIgnore] 
        public int usedCount = 0;

        public Card(int id, Type type, int rank, CardCondition condition, CardEffect effect, Dictionary<string,Variable> variable)
        {
            this.id = id;
            this.type = type;
            this.rank = rank;
            this.condition = null;
            this._condition = condition;
            this.effect = effect;
            this._variable = variable;
            this.variable = null;
            if (variable.Count > 0)
                this.variable = variable;
        }
        
        public Card Clone()
        {
            return (Card)this.MemberwiseClone();
        }
        
        public CardEffect.Result[] UseEffect(GamePlayer gamePlayer, bool isDiscard = false)
        {
            usedCount++;
            var results = isDiscard ? effect.UseLeak(this, gamePlayer) : effect.Use(this, gamePlayer);
            return results;
        }

        public bool CheckCondition(List<Resource.Type> resource)
        {
            return _condition.Check(resource);
        }
        
    }
}
