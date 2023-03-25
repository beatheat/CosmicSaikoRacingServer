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

		/// <summary>
		/// 랜덤한 리소스릴 조건을 생성한다
		/// </summary>
		private void MakeRandomCondition()
		{
			List<Resource.Type> lockCondition;
			do
			{
				int conditionCount = count switch
				{
					>= 1 and <= 3 => 2,
					>= 4 and <= 6 => 3,
					>= 7 and <= 9 => 4,
					>= 10 => 5,
					_ => 0
				};
				lockCondition = new List<Resource.Type>();
				for (int i = 0; i < conditionCount; i++)
					lockCondition.Add(Util.GetRandomEnumValue<Resource.Type>());
			} while (lockCondition.IsSame(player.resourceReel));
			resourceCondition = lockCondition;
		}

		/// <summary>
		/// 턴 시작시 증식버프가 있다면 적용한다
		/// </summary>
		public override void OnTurnStart()
		{
			if (count == 0) return;
			MakeRandomCondition();
		}


		/// <summary>
		/// 리소스 리롤 후 증식버프의 조건을 만족하면 증식버프스택을 0으로 만든다
		/// </summary>
		public override void AfterRerollResource(ref List<int>? resourceFixed, ref List<Resource.Type> resourceReel)
		{
			if (player.resourceReel.Contains(resourceCondition))
			{
				resourceCondition.Clear();
				count = 0;
			}
		}
		

		/// <summary>
		/// 증식버프 조건을 만족하지 않았다면 카드 사용불가
		/// </summary>
		public override bool BeforeUseCard(ref Card card, ref CardEffectModule.Result[] results)
		{
			return count == 0;
		}
		
		/// <summary>
		/// 카드 사용후 증식버프스택에 변동이 있으면 적용
		/// </summary>
		public override void AfterUseCard(ref Card card)
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