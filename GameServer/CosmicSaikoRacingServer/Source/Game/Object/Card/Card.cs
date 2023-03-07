﻿using System.Text.Json.Serialization;

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
        public Dictionary<string, Variable> variable;
        
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
        public int usedCount = 0;

        public Card(int id, Type type, int rank, CardCondition condition, CardEffect effect, Dictionary<string,Variable> variable)
        {
            this.id = id;
            this.type = type;
            this.rank = rank;
            this.condition = condition;
            this.effect = effect;
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

        public bool CheckCondition(List<ResourceType> resource)
        {
            return condition.Check(resource);
        }
        
    }
}
