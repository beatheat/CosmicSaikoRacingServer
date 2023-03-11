namespace CSRServer.Game
{
	internal class BuffExposure : Buff
	{
		public List<Card> exposureCards;
		public BuffExposure(GamePlayer player) : base(player)
		{
			exposureCards = new List<Card>();
			type = Buff.Type.Exposure;
		}

		public CardCondition GetRandomCondition()
		{
			CardCondition cardCondition;
			Random random = new Random();
			//same
			if (random.Next(2) == 0)
			{
				int randomNumber = random.Next(2, 7);
				if (randomNumber == 6)
				{
					cardCondition = new CardCondition(22);
				}
				else
				{
					cardCondition = new CardCondition(randomNumber);
				}
			}
			else
			{
				int randomNumber = random.Next(1, 6);
				var lockCondition = new List<Resource.Type>(randomNumber);
				for (int j = 0; j < randomNumber; j++)
					lockCondition[j] = Util.GetRandomEnumValue<Resource.Type>();
				cardCondition = new CardCondition(lockCondition);
			}
			
			return cardCondition;
		}

		public void SetExposure(Card card)
		{
			card.isExposure = true;
			card.condition = GetRandomCondition();
			exposureCards.Add(card);
		}
		
		
		public override void OnTurnStart()
		{
			if (count == 0) return;

			//패 수만큼만 피폭
			int exposureCount = count > player.hand.Count ? player.hand.Count : count;
			
			//랜덤 패에 부여
			Util.DistributeOnList(player.hand, exposureCount, out var selectedCards);
			foreach (var card in selectedCards)
			{
				SetExposure(card);
			}
		}

		public override void OnDrawCard(ref Card card)
		{
			if (count == 0) return;
			if (count - player.hand.Count > 0)
			{
				SetExposure(card);
			}
		}

		public override bool BeforeUseCard(ref Card card)
		{
			if (count > 0 && card.isExposure)
			{
				count--;
				exposureCards.Remove(card);
			}
			return true;
		}
		
		
		public override void AfterUseCard(ref Card card, ref CardEffect.Result[] results)
		{
			if(!(exposureCards.Count < count && exposureCards.Count < player.hand.Count))
				return;

			var nonExposureHand = player.hand.FindAll(x => x.isExposure == false);
			Util.DistributeOnList(nonExposureHand, 1, out var selectedCards);
			SetExposure(selectedCards[0]);
		}

		public override int Release()
		{
			foreach (var card in exposureCards)
			{
				card.isExposure = false;
				card.condition = CardManager.GetCard(card.id).condition;
			}
			exposureCards.Clear();
			return base.Release();
		}
	}
}