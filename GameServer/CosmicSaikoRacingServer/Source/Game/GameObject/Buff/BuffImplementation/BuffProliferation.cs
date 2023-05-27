using CSR.Game.Phase;
using CSR.Game.Player;
using ProtoBuf;

namespace CSR.Game.GameObject;


[ProtoContract]
internal class BuffProliferation : Buff
{
	[ProtoMember(3)]
	public List<ResourceType> ResourceCondition { get; set; }

	public BuffProliferation(PreheatPhase phase, GamePlayer player) : base(phase, player)
	{
		Type = BuffType.Proliferation;
		ResourceCondition = new List<ResourceType>();
	}

	/// <summary>
	/// 랜덤한 리소스릴 조건을 생성한다
	/// </summary>
	private void MakeRandomCondition()
	{
		List<ResourceType> lockCondition;
		do
		{
			int conditionCount = Count switch
			{
				>= 1 and <= 3 => 2,
				>= 4 and <= 6 => 3,
				>= 7 and <= 9 => 4,
				>= 10 => 5,
				_ => 0
			};
			lockCondition = new List<ResourceType>();
			for (int i = 0; i < conditionCount; i++)
				lockCondition.Add(Util.GetRandomEnumValue<ResourceType>());
		} while (player.Resource.Reel.Contains(lockCondition));

		ResourceCondition = lockCondition;
	}

	/// <summary>
	/// 턴 시작시 증식버프가 있다면 적용한다
	/// </summary>
	public override void OnPreheatStart()
	{
		if (Count == 0) return;
		MakeRandomCondition();
	}


	/// <summary>
	/// 리소스 리롤 후 증식버프의 조건을 만족하면 증식버프스택을 0으로 만든다
	/// </summary>
	public override void AfterRerollResource(List<int>? resourceFixed)
	{
		if (player.Resource.Reel.Contains(ResourceCondition))
		{
			ResourceCondition.Clear();
			Count = 0;
		}
	}


	/// <summary>
	/// 증식버프 조건을 만족하지 않았다면 카드 사용불가
	/// </summary>
	public override bool BeforeUseCard(Card card, ref CardEffectModule.Result[] results)
	{
		return Count == 0;
	}

	/// <summary>
	/// 카드 사용후 증식버프스택에 변동이 있으면 적용
	/// </summary>
	public override void AfterUseCard(Card card)
	{
		if (ResourceCondition.Count >= Count)
			return;

		MakeRandomCondition();
	}


	public override int Release()
	{
		ResourceCondition = new List<ResourceType>();
		return base.Release();
	}
}