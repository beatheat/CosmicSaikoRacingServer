namespace CSRServer.Game
{
	internal class BuffRefine : Buff
	{
		private const int REFINE_COEFFICIENT = 5;
		public BuffRefine(GamePlayer player) : base(player)
		{
			type = Buff.Type.Refine;
		}

		public override void AfterRollResource(ref List<int>? resourceFixed, ref List<Resource.Type> resourceReel)
		{
			int cardTypeCount = Enum.GetValues(typeof(Card.Type)).Length;
			int[] frequentList = Enumerable.Repeat<int>(0,cardTypeCount).ToArray();
				
			foreach (var card in player.hand)
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
			

			int resourceTypeCount = Enum.GetValues((typeof(Resource.Type))).Length;
			double highPercentage = 100.0 / player.resourceReelCount + REFINE_COEFFICIENT * count;
			double[] resourcePercentage = Enumerable.Repeat( (100.0-highPercentage) / (resourceTypeCount-1), player.resourceReelCount).ToArray();
			resourcePercentage[mostFqCardType] = highPercentage;
			
			for (int i = 0; i < player.resourceReelCount; i++)
			{
				Resource.Type resource = Util.GetRandomEnumValue<Resource.Type>(resourcePercentage);
				if (i >= resourceReel.Count)
					resourceReel.Add(resource);
				else if (resourceFixed != null && !resourceFixed.Contains(i))
					resourceReel[i] = resource;
			}

			
			
		}
		
		public override void OnTurnEnd()
		{
			
		}
	}
}