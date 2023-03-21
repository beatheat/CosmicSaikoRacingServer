

namespace CSRServer.Game
{
	/// <summary>
	/// 카드의 리소스 조건을 표현하는 클래스
	/// </summary>
	public class CardCondition
	{
		/// <summary>
		/// 페어류 리소스 조건 타입
		/// </summary>
		public enum CommonType
		{
			None=0, OnePair=2, Triple, FourCard, Yacht, TwoPair = 22
		}
		
		public readonly List<Resource.Type> conditionList;
		public readonly CommonType type;
		public readonly bool isCommon;
		public readonly int count;

		//CommonType이 아닐 경우 생성자
		public CardCondition(List<Resource.Type> conditionList)
		{
			this.conditionList = conditionList;
			this.isCommon = false;
			type = CommonType.None;
			count = 0;
		}
		
		//CommonType일 경우 생성자
		public CardCondition(int sameCount = 0)
		{
			this.conditionList = null!;
			this.isCommon = true;
			this.count = sameCount;
			this.type = (CommonType) sameCount;
		}
		
		//리소스 조건을 만족하는지 검사
		public bool Check(List<Resource.Type> resourceReel)
		{
			//CommonType이 아닐 경우
			if (isCommon == false)
			{
				return resourceReel.Contains(conditionList);
			}
			//CommonType일 경우
			else
			{
				//투페어
				if (count == 22)
				{
					int pairCount = 0;
					int[] resourceCount = Enumerable.Repeat(0, Resource.COUNT).ToArray();
					// 리소스 개수를 종류별로 센다
					foreach (var resource in resourceReel)
					{
						resourceCount[(int) resource]++;
					}
					// 2개 이상인 리소스가 있으면 pairCount에 더한다
					for (int i = 0; i < Resource.COUNT; i++)
					{
						if (resourceCount[i] >= 2)
							pairCount++;
					}
					//최종적으로 pairCount가 2이상이면 투페어
					return pairCount >= 2;
				}
				else
				{
					//페어,트리플,포카드,야추
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