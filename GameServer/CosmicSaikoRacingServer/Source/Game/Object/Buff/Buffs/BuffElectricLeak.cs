

namespace CSRServer.Game
{
	internal class BuffElectricLeak : Buff
	{
		public List<int> resourceLockIndexList;
		
		public BuffElectricLeak(GamePlayer player) : base(player)
		{
			type = Buff.Type.ElectricLeak;
			resourceLockIndexList = new List<int>();
		}

		public void LockResources(int count)
		{
			var remainResourceReel = Enumerable.Range(0, player.resourceReelCount).Except(resourceLockIndexList).ToList();
			Util.DistributeOnList(remainResourceReel, count, out resourceLockIndexList);
		}
		
		public override void OnTurnStart()
		{
			if (count == 0) return;
			
			int lockCount = count > player.resourceReelCount ?  player.resourceReelCount : count;

			LockResources(lockCount);
		}
		

		public override void AfterUseCard(ref Card card, ref CardEffect.Result[] results)
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

		public override void OnRollResource(ref List<int>? resourceFixed)
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