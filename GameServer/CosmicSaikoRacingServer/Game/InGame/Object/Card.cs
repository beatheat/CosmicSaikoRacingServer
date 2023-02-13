using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CSRServer
{
    internal class Card
    {
        public int id;
        
        public class Condition
        {
            public bool all = false;
            public ResourceType type;
            public int count;
        }
        
        [JsonIgnore] 
        public List<Condition> condition = new List<Condition>();

        [JsonIgnore]
        public bool canUse = false;

        [JsonIgnore]
        public List<CardEffectType> effectType;
        
        [JsonIgnore] 
        public int rank;
        [JsonIgnore] 
        public ResourceType type; // 타입이 아니라 조건
        [JsonIgnore] 
        public bool death = false;
        [JsonIgnore] 
        public int percentage = 0;
        [JsonIgnore] 
        public int usedCount = 0;
        [JsonIgnore] 
        public int useCountAtOnes = 1;

        public object[] UseEffect(GamePlayer gamePlayer)
        {
            object[] results = new object[useCountAtOnes];
            for (int i = 0; i < useCountAtOnes; i++)
            {
                results[i] = Effect(gamePlayer);
                usedCount++;
            }
            useCountAtOnes = 1;
            return results;
        }

        public virtual object Effect(GamePlayer gamePlayer)
        {
            return null!;
        }

        // public virtual void Enforce(GamePlayer gamePlayer) { }

        // public static void Add(GamePlayer player, ResourceType type, int amount, int rank)
        // {
        //     foreach (var res in player.handResource)
        //     {
        //         if (res.rank <= rank && (type == Resource.Type.ALL || res.type == type))
        //         {
        //             res.magnitude += amount;
        //         }
        //     }
        // }
        //
        // public static void Multiply(GamePlayer player, Resource.Type type, int amount, int rank)
        // {
        //     foreach (var res in player.handResource)
        //     {
        //         if (res.rank <= rank && (type == Resource.Type.ALL || res.type == type))
        //         {
        //             res.magnitude *= amount;
        //         }
        //     }
        // }
        //
        // public static void Coin(GamePlayer player, int amount)
        // {
        //     player.coin += amount;
        // }
        //
        // public static void Death(Card card)
        // {
        //     card.death = true;
        // }
        //
        // public static bool Overload(GamePlayer player, Card card, Action effect, int amount)
        // {
        //     card.percentage += amount;
        //     Random random = new Random();
        //     if (random.Next(100) < card.percentage)
        //     {
        //         effect();
        //         return true;
        //     }
        //     return false;
        // }
        //
        // public static bool Delay(GamePlayer player, Card card, Action effect, int count)
        // {
        //     if (card.usedCount >= count)
        //     {
        //
        //     }
        //     Random random = new Random();
        //     if (random.Next(100) < card.percentage)
        //     {
        //         effect();
        //         return true;
        //     }
        //     return false;
        // }
        //
        // public static void Create(GamePlayer player, int id, int num)
        // {
        // }
    }
}
