namespace CSRServer.Game
{
    public static class Resource
    {
        //리소스 종류 개수
        public const int COUNT = 5;
        public enum Type
        {
            Fossil, Electric, Bio, Nuclear, Cosmic
        }

        /// <summary>
        /// resourceA와 resourceB 리스트를 구성하는 리소스가 동일한 지 확인
        /// </summary>
        public static bool IsSame(this List<Type> resourceA, List<Type> resourceB)
        {
            if (resourceA.Count != resourceB.Count) return false;
            var resourceAClone = new List<Type>(resourceA);
            var resourceBClone = new List<Type>(resourceB);
            resourceA.Sort();
            resourceB.Sort();
            //정렬한 두 리스트가 동일한지 확인
            bool check = true;
            for (int i = 0; i < resourceA.Count; i++)
            {
                check = check && resourceAClone[i] == resourceBClone[i];
            }
            return check;
        }
        
        /// <summary>
        /// part의 구성 리소스가 origin에 포함하는지 확인
        /// </summary>
        public static bool Contains(this List<Type> origin, List<Type>? part)
        {
            if (part == null) return false;
            if (origin.Count  < part.Count) return false;
            int[] originCount = Enumerable.Repeat(0, COUNT).ToArray();
            int[] partCount = Enumerable.Repeat(0, COUNT).ToArray();
            
            //각 리소스의 개수 세기
            foreach (var resource in origin)
                originCount[(int) resource]++;
            foreach (var resource in part)
                partCount[(int) resource]++;

            //part의 구성리소스가 origin에 포함하는지 확인
            bool check = true;
            for (int i = 0; i < originCount.Length; i++)
            {
                check = check && originCount[i] >= partCount[i];
            }
            return check;
        }
        
    }
}
