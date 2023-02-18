using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSRServer
{
    internal static class Util
    {
        /// <summary>
        /// big리스트에 있는 값을 size개 만큼 small리스트에 랜덤으로 옮긴다
        /// </summary>
        public static bool DistributeOnList<T>(List<T> small, List<T> big, int size)
        {
            if (size > big.Count)
            {
                return false;
            }
            small.Clear();
            List<T> bigClone = new List<T>(big);
            Random random = new Random();
            for (int i = 0; i < size; i++)
            {
                int randomNum = random.Next(bigClone.Count);
                var selected = bigClone[randomNum];
                bigClone.Remove(selected);
                small.Add(selected);
            }
            return true;
        }

        public static bool DistributeAndMoveOnList<T>(List<T> small, List<T> big, int size)
        {
            if (size > big.Count)
            {
                return false;
            }
            small.Clear();
            Random random = new Random();
            for (int i = 0; i < size; i++)
            {
                int randomNum = random.Next(big.Count);
                var selected = big[randomNum];
                big.Remove(selected);
                small.Add(selected);
            }
            return true;
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            (a, b) = (b, a);
        }

        public static bool Swap<T>(this List<T> list, int from, int to)
        {
            if (from >= list.Count || from < 0 || to >= list.Count || to < 0)
                return false;
            (list[from], list[to]) = (list[to], list[from]);
            return true;
        }
        
        public static T GetRandomEnumValue<T>()
        {
            Array values = Enum.GetValues(typeof(T));
            Random random = new Random();
            T randomEnumValue = (T)values.GetValue(random.Next(values.Length))!;
            return randomEnumValue;
        }
    }
}
