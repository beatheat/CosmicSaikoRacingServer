

namespace CSRServer.Game
{
	public struct CardCondition
	{
		public enum CommonType
		{
			None=0, OnePair=2, Triple, FourCard, Yacht, TwoPair
		}
		
		public List<Resource.Type> conditionList;
		public CommonType type;
		public bool isCommon;
		public int count;

		public CardCondition(List<Resource.Type> conditionList)
		{
			this.conditionList = conditionList;
			this.isCommon = false;
			type = CommonType.None;
			count = 0;
		}
		
		public CardCondition(int sameCount = 0)
		{
			this.conditionList = null!;
			this.isCommon = true;
			this.count = sameCount;
			if (sameCount < 10)
				this.type = (CommonType) sameCount;
			else
				this.type = CommonType.TwoPair;
		}
		
		public bool Check(List<Resource.Type> resourceReel)
		{
			if (isCommon == false)
			{
				return resourceReel.Contains(conditionList);
			}
			else
			{
				//투페어
				if (count == 22)
				{
					int pairCount = 0;
					int[] resourceCount = Enumerable.Repeat(0, Resource.COUNT).ToArray();
					foreach (var resource in resourceReel)
					{
						resourceCount[(int) resource]++;
					}

					for (int i = 0; i < Resource.COUNT; i++)
					{
						if (resourceCount[i] >= 2)
							pairCount++;
					}
					return pairCount >= 2;
				}
				else
				{
					foreach (var resourceType in Enum.GetValues<Resource.Type>())
					{
						int sameCount = 0;
						foreach (var resource in resourceReel)
						{
							if (resourceType == resource)
								sameCount++;
						}
						if (sameCount >= this.count)
							return true;
					}
					return false;
				}
			}
		}
	}
}