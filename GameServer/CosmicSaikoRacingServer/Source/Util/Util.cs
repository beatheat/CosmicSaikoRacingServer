namespace CSR;

internal static class Util
{
    /// <summary>
    /// big리스트에 있는 값들을 size개 만큼 small리스트에 랜덤으로 옮긴다, size가 big.count보다 클경우 최대값에 맞춘다
    /// </summary>
    public static bool DistributeOnList<T>(List<T> big, int size, out List<T> small)
    {
        small = new List<T>();
        if (size > big.Count)
            size = big.Count;

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

    /// <summary>
    /// big리스트에 있는 값들을 size개 만큼 small리스트에 랜덤으로 잘라내서 옮긴다
    /// </summary>
    public static bool DistributeAndMoveOnList<T>(List<T> big, int size, out List<T> small)
    {
        small = new List<T>();
        if (size > big.Count)
        {
            return false;
        }

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

    /// <summary>
    /// min과 max 사이의 랜덤한 값을 가진 리스트를 반환한다. 중복되는 값은 없다
    /// </summary>
    public static List<int> GetRandomNumbers(int count, int min, int max)
    {
        if (count > max - min)
            return new List<int>();
        Random random = new Random();
        List<int> randomNumbers = new List<int>(count);
        int[] arr = new int[3];
        for (int i = 0; i < count; i++)
        {
            int randomNumber;
            do
            {
                randomNumber = random.Next(min, max);
            } while (randomNumbers.FindIndex(x => x == randomNumber) < 0);

            randomNumbers[i] = randomNumber;
        }

        return randomNumbers;
    }

    /// <summary>
    /// 0과 max 사이의 랜덤한 값을 가진 리스트를 반환한다. 중복되는 값은 없다
    /// </summary>
    /// <param name="count"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static List<int> GetRandomNumbers(int count, int max)
    {
        return GetRandomNumbers(count, 0, max);
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

    /// <summary>
    /// 특정 Enum타입의 값을 랜덤으로 반환
    /// </summary>
    public static T GetRandomEnumValue<T>() where T : struct, Enum
    {
        Array values = Enum.GetValues(typeof(T));
        Random random = new Random();
        T randomEnumValue = (T) values.GetValue(random.Next(values.Length))!;
        return randomEnumValue;
    }

    //{10,20,70} 일렇게 넣으면 차지하고 있는 구간만큼의 확률로 나옴
    /// <summary>
    /// 특정 Enum타입의 값을 percentage 배열의 확률만큼 반환
    /// </summary>
    public static T GetRandomEnumValue<T>(double[] percentage) where T : struct, Enum
    {
        Array values = Enum.GetValues(typeof(T));
        if (percentage.Length != values.Length)
            return default(T);

        Random random = new Random();
        int randomNumber = random.Next(100);
        for (int i = 0; i < values.Length; i++)
        {
            if (i > 0)
                percentage[i] += percentage[i - 1];

            if (randomNumber < percentage[i])
            {
                return (T) values.GetValue(i)!;
            }
        }

        return default(T);
    }

}