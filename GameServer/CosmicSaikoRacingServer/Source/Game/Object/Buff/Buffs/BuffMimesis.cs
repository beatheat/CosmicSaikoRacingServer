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

		/// <summary>
		/// 의태 버프를 적용한다
		/// </summary>
		private void SetMimesis(Card card)
		{
			card.isExposure = true;
			card.condition = new CardCondition(0);
			mimesisCards.Add(card);
		}
		
		/// <summary>
		/// 턴 시작 시 의태버프가 존재하면 적용한다
		/// </summary>
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
		
		/// <summary>
		/// 카드 드로우 시 남은 의태버프가 있으면 적용한다
		/// </summary>
		public override void OnDrawCard(ref Card card)
		{
			if (count == 0) return;
			if (count - player.hand.Count > 0)
			{
				SetMimesis(card);
			}
		}

		/// <summary>
		/// 카드 사용 시 의태한 카드면 의태버프스택 1 감소한다
		/// </summary>
		public override bool BeforeUseCard(ref Card card)
		{
			if (count > 0 && card.isExposure)
			{
				count--;
				mimesisCards.Remove(card);
			}
			return true;
		}
		
		/// <summary>
		/// 카드 사용 후 의태 버프에 변동이 있으면 적용한다
		/// </summary>
		public override void AfterUseCard(ref Card card, ref CardEffectModule.Result[] results)
		{
			//의태한 카드 수가 의태버프스택보다 적고 손패의 개수보다 많을때만 발동 
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