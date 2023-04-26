namespace CSRServer.Game
{
	internal class BuffRefine : Buff
	{
		private const int REFINE_COEFFICIENT = 5;
		public BuffRefine(GamePlayer player) : base(player)
		{
			type = Buff.Type.Refine;
		}

		/// <summary>
		/// 리롤후 정제버프가 존재한다면 적용한다
		/// </summary>
		public override void AfterRerollResource(ref List<int>? resourceFixed, ref List<Resource.Type> resourceReel)
		{
			if(count == 0) return;
			int cardTypeCount = Enum.GetValues(typeof(Card.Type)).Length;
			int[] frequentList = Enumerable.Repeat<int>(0,cardTypeCount).ToArray();

			//현재 패에서 가장 많은 카드 타입을 찾는다
			foreach (var card in player.cardSystem.hand)
			{
				frequentList[(int) card.type]++;
			}

			int mostFqCount = 0;
			int mostFqCardType = 0;
			for (int i = 0; i < cardTypeCount; i++)
			{
				if (mostFqCount > frequentList[i])
				{
					mostFqCount = frequentList[i];
					mostFqCardType = i;
				}
			}

			if (mostFqCardType == (int) Card.Type.Normal)
				return;
			
			//가장 많은 타입이 리롤시 가장 등장할 확률이 높다

			int resourceTypeCount = Enum.GetValues((typeof(Resource.Type))).Length;
			double highPercentage = 100.0 / Resource.COUNT + REFINE_COEFFICIENT * count;
			double[] resourcePercentage = Enumerable.Repeat( (100.0-highPercentage) / (resourceTypeCount-1), Resource.COUNT).ToArray();
			resourcePercentage[mostFqCardType] = highPercentage;
			
			for (int i = 0; i < player.resourceSystem.reelCount; i++)
			{
				Resource.Type resource = Util.GetRandomEnumValue<Resource.Type>(resourcePercentage);
				if (i >= resourceReel.Count)
					resourceReel.Add(resource);
				else if (resourceFixed == null || !resourceFixed.Contains(i))
					resourceReel[i] = resource;
			}
		}
		
	}
}