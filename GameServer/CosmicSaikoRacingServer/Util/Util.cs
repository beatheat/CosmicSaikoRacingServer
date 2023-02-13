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
        static public bool DistributeOnList<T>(List<T> small, List<T> big, int size)
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

        static public bool DistributeAndMoveOnList<T>(List<T> small, List<T> big, int size)
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

        static public void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        static public bool Swap<T>(this List<T> list, int from, int to)
        {
            if (from >= list.Count || from < 0 || to >= list.Count || to < 0)
                return false;
            T temp = list[from];
            list[from] = list[to];
            list[to] = temp;
            return true;
        }
    }
}
