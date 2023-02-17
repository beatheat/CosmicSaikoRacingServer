

namespace CSRServer.Game
{
	internal class CardCondition
	{
		private readonly List<ResourceType> conditionList;
		private readonly List<int> countList;
		private readonly bool freeSameCondtion;
		private readonly int freeSameCount;

		public CardCondition(List<ResourceType> conditionList, List<int> countList)
		{
			this.conditionList = conditionList;
			this.freeSameCondtion = false;
			this.countList = countList;
		}
		
		public CardCondition(int sameCount)
		{
			this.conditionList = null!;
			this.countList = null!;
			this.freeSameCondtion = true;
			this.freeSameCount = sameCount;
		}
		
		public bool Check(List<ResourceType> resource)
		{
			if (freeSameCondtion == false)
			{
				for (int i = 0; i < conditionList.Count; i++)
				{
					var resourceType = conditionList[i];
					int count = 0;
					foreach (var resourceElement in resource)
					{
						if (resourceType == resourceElement)
							count++;
					}
					if (count >= countList[i])
						return true;
				}
				return false;
			}
			else
			{
				//투페어 , 혹시 쓰리페어?
				if (freeSameCount == 22)
				{
					int pairCount = 0;
					foreach (var resourceType in Enum.GetValues<ResourceType>())
					{
						int count = 0;
						foreach (var resourceElement in resource)
						{
							if (resourceType == resourceElement)
								count++;
						}
						if (count >= 2)
							pairCount++;
					}
					return pairCount >= 2;
				}
				else
				{
					foreach (var resourceType in Enum.GetValues<ResourceType>())
					{
						int count = 0;
						foreach (var resourceElement in resource)
						{
							if (resourceType == resourceElement)
								count++;
						}
						if (count >= freeSameCount)
							return true;
					}
					return false;
				}
			}
		}
	}
}