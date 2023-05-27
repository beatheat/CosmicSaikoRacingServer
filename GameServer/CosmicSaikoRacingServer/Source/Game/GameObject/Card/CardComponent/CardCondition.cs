using ProtoBuf;

namespace CSR.Game.GameObject;

/// <summary>
/// 카드의 리소스 조건을 표현하는 클래스
/// </summary>

[ProtoContract]
public class CardCondition
{
	/// <summary>
	/// 페어류 리소스 조건 타입
	/// </summary>
	public enum CommonType
	{
		None = 0,
		OnePair = 2,
		Triple,
		FourCard,
		Yacht,
		TwoPair = 22
	}
	
	[ProtoMember(1)]
	public List<ResourceType> ConditionList { get; }
	[ProtoMember(2)]
	public CommonType Type { get; }
	[ProtoMember(3)]
	public bool IsCommon { get; }
	[ProtoMember(4)]
	public int Count { get; }

	//CommonType이 아닐 경우 생성자
	public CardCondition(List<ResourceType> conditionList)
	{
		this.ConditionList = conditionList;
		this.IsCommon = false;
		Type = CommonType.None;
		Count = 0;
	}

	//CommonType일 경우 생성자
	public CardCondition(int sameCount = 0)
	{
		this.ConditionList = null!;
		this.IsCommon = true;
		this.Count = sameCount;
		this.Type = (CommonType) sameCount;
	}

	//리소스 조건을 만족하는지 검사
	public bool Check(List<ResourceType> resourceReel)
	{
		//CommonType이 아닐 경우
		if (IsCommon == false)
		{
			return resourceReel.Contains(ConditionList);
		}
		//CommonType일 경우
		else
		{
			//투페어
			if (Count == 22)
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
				foreach (var resourceType in Enum.GetValues<ResourceType>())
				{
					int sameCount = 0;
					foreach (var resource in resourceReel)
					{
						if (resourceType == resource)
							sameCount++;
					}

					if (sameCount >= this.Count)
						return true;
				}

				return false;
			}
		}
	}
}