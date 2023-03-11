namespace CSRServer.Game
{
	internal class BuffMimesis : Buff
	{
		public List<Card> mimesisCards;

		public BuffMimesis(GamePlayer player) : base(player)
		{
			type = Buff.Type.Mimesis;
			mimesisCards = new List<Card>();
		}

		public void SetMimesis(Card card)
		{
			card.isExposure = true;
			card.condition = new CardCondition(0);
			mimesisCards.Add(card);
		}
		
		public override void OnTurnStart()
		{
			if (count == 0) return;
			

			//최대 패 수 만큼 의태
			int mimesisCount = count > player.hand.Count ? player.hand.Count : count;

			
			//랜덤 패에 부여
			Util.DistributeOnList(player.hand, mimesisCount, out var selectedCards);
			foreach (var card in selectedCards)
			{
				SetMimesis(card);
			}
		}
		
		
		public override void OnDrawCard(ref Card card)
		{
			if (count == 0) return;
			if (count - player.hand.Count > 0)
			{
				SetMimesis(card);
			}
		}

		public override bool BeforeUseCard(ref Card card)
		{
			if (count > 0 && card.isExposure)
			{
				count--;
				mimesisCards.Remove(card);
			}
			return true;
		}
		
		public override void AfterUseCard(ref Card card, ref CardEffect.Result[] results)
		{
			if(!(mimesisCards.Count < count && mimesisCards.Count < player.hand.Count))
				return;

			var nonExposureHand = player.hand.FindAll(x => x.isMimesis == false);
			Util.DistributeOnList(nonExposureHand, 1, out var selectedCards);
			SetMimesis(selectedCards[0]);
		}


		public override int Release()
		{
			foreach (var card in mimesisCards)
			{
				card.isExposure = false;
				card.condition = CardManager.GetCard(card.id).condition;
			}
			mimesisCards.Clear();
			return base.Release();
		}
	}
}