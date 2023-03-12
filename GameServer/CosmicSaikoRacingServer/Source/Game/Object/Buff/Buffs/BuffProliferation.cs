namespace CSRServer.Game
{
	internal class BuffProliferation : Buff
	{
		public List<Resource.Type> resourceCondition;

		public BuffProliferation(GamePlayer player) : base(player)
		{
			type = Buff.Type.Proliferation;
			resourceCondition = new List<Resource.Type>();
		}

		private void MakeRandomCondition()
		{
			int conditionCount = count switch
			{
				>= 1 and <= 3 => 2,
				>= 4 and <= 6 => 3,
				>= 7 and <= 9 => 4,
				>= 10 => 5,
				_ => 0
			};
			List<Resource.Type> lockCondition = new List<Resource.Type>();
			for (int i = 0; i < conditionCount; i++)
				lockCondition.Add(Util.GetRandomEnumValue<Resource.Type>());
			resourceCondition = lockCondition;
		}

		public override void OnTurnStart()
		{
			if (count == 0) return;
			MakeRandomCondition();
		}

		public override void BeforeRollResource(ref List<int>? resourceFixed)
		{
			if (count == 0) return;
			if (player.resourceReel.Contains(resourceCondition))
				count = 0;
		}

		public override bool BeforeUseCard(ref Card card)
		{
			return count == 0;
		}
		
		public override void AfterUseCard(ref Card card, ref CardEffect.Result[] results)
		{
			if(resourceCondition.Count >= count)
				return;

			MakeRandomCondition();
		}


		public override int Release()
		{
			resourceCondition = new List<Resource.Type>();
			return base.Release();
		}
	}
}