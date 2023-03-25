

namespace CSRServer.Game
{
	internal class BuffElectricLeak : Buff
	{
		/// <summary>
		/// 누전된 리소스릴 인덱스
		/// </summary>
		public List<int> resourceLockIndexList;
		
		public BuffElectricLeak(GamePlayer player) : base(player)
		{
			type = Buff.Type.ElectricLeak;
			resourceLockIndexList = new List<int>();
		}

		/// <summary>
		/// 랜덤한 리소스릴을 잠근다
		/// </summary>
		private void LockResources(int count)
		{
			var remainResourceReel = Enumerable.Range(0, player.resourceReelCount).Except(resourceLockIndexList).ToList();
			Util.DistributeOnList(remainResourceReel, count, out var additionalLockIndexList);
			resourceLockIndexList.AddRange(additionalLockIndexList);
		}
		
		/// <summary>
		/// 턴 시작시 누전버프가 있다면 누전적용
		/// </summary>
		public override void OnTurnStart()
		{
			if (count == 0) return;
			
			int lockCount = count > player.resourceReelCount ?  player.resourceReelCount : count;

			LockResources(lockCount);
		}
		
		/// <summary>
		/// 카드 사용 후 누전 버프 변동이 있다면 누전적용
		/// </summary>
		public override void AfterUseCard(ref Card card)
		{
			//누전된 리소스가 리소스 릴 최대치 보다 적고 버프카운트가 이전에 누전된 리소스보다 많을 때만 작동함 
			if (!(resourceLockIndexList.Count < player.resourceReelCount && resourceLockIndexList.Count < count)) 
				return;

			int lockCount = count - resourceLockIndexList.Count;
			int remainResourceReelCount = player.resourceReelCount - resourceLockIndexList.Count;
			//최대로, 남은 리소스릴 개수만큼만 누전됨
			lockCount = lockCount < remainResourceReelCount ? lockCount : remainResourceReelCount;
			LockResources(lockCount);
		}

		/// <summary>
		/// 누전이 있다면 리소스 리롤시 리롤이 안됨
		/// </summary>
		public override void BeforeRerollResource(ref List<int>? resourceFixed)
		{
			if (count == 0) return;
			resourceFixed?.AddRange(resourceLockIndexList);
		}

		
		public override int Release()
		{
			resourceLockIndexList = new List<int>();
			return base.Release();
		}

	}
}