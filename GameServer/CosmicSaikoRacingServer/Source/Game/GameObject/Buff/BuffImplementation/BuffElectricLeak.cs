using CSR.Game.Phase;
using ProtoBuf;

namespace CSR.Game.GameObject;

[ProtoContract]
internal class BuffElectricLeak : Buff
{
	/// <summary>
	/// 누전된 리소스릴 인덱스
	/// </summary>
	[ProtoMember(3)]
	public List<int> ResourceLockIndexList { get; set; }

	public BuffElectricLeak(PreheatPhase phase, GamePlayer player) : base(phase, player)
	{
		Type = BuffType.ElectricLeak;
		ResourceLockIndexList = new List<int>();
	}

	/// <summary>
	/// 랜덤한 리소스릴을 잠근다
	/// </summary>
	private void LockResources(int count)
	{
		var remainResourceReel = Enumerable.Range(0, player.Resource.ReelCount).Except(ResourceLockIndexList).ToList();
		Util.DistributeOnList(remainResourceReel, count, out var additionalLockIndexList);
		ResourceLockIndexList.AddRange(additionalLockIndexList);
	}

	/// <summary>
	/// 턴 시작시 누전버프가 있다면 누전적용
	/// </summary>
	public override void OnPreheatStart()
	{
		if (Count == 0) return;

		int lockCount = Count > player.Resource.ReelCount ? player.Resource.ReelCount : Count;

		LockResources(lockCount);
	}

	/// <summary>
	/// 카드 사용 후 누전 버프 변동이 있다면 누전적용
	/// </summary>
	public override void AfterUseCard(Card card)
	{
		//누전된 리소스가 리소스 릴 최대치 보다 적고 버프카운트가 이전에 누전된 리소스보다 많을 때만 작동함 
		if (!(ResourceLockIndexList.Count < player.Resource.ReelCount && ResourceLockIndexList.Count < Count))
			return;

		int lockCount = Count - ResourceLockIndexList.Count;
		int remainResourceReelCount = player.Resource.ReelCount - ResourceLockIndexList.Count;
		//최대로, 남은 리소스릴 개수만큼만 누전됨
		lockCount = lockCount < remainResourceReelCount ? lockCount : remainResourceReelCount;
		LockResources(lockCount);
	}

	/// <summary>
	/// 누전이 있다면 리소스 리롤시 리롤이 안됨
	/// </summary>
	public override void BeforeRerollResource(ref List<int>? resourceFixed)
	{
		if (Count == 0) return;
		resourceFixed?.AddRange(ResourceLockIndexList);
	}


	public override int Release()
	{
		ResourceLockIndexList = new List<int>();
		return base.Release();
	}

}