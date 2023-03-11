namespace CSRServer.Game
{
    public static class Resource
    {
        public const int COUNT = 5;
        public enum Type
        {
            Fossil, Electric, Bio, Nuclear, Cosmic
        }

        public static bool IsSame(this List<Type> resourceA, List<Type> resourceB)
        {
            if (resourceA.Count != resourceB.Count) return false;
            var resourceAClone = new List<Type>(resourceA);
            var resourceBClone = new List<Type>(resourceB);
            resourceA.Sort();
            resourceB.Sort();
            bool check = true;
            for (int i = 0; i < resourceA.Count; i++)
            {
                check = check && resourceAClone[i] == resourceBClone[i];
            }
            return check;
        }
        
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

            bool check = true;
            for (int i = 0; i < origin.Count; i++)
            {
                check = check && originCount[i] >= partCount[i];
            }
            return check;
        }
        
    }
}
