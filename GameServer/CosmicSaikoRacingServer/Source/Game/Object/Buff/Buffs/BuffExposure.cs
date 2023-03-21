﻿namespace CSRServer.Game
{
	internal class BuffExposure : Buff
	{
		public List<Card> exposureCards;
		public BuffExposure(GamePlayer player) : base(player)
		{
			exposureCards = new List<Card>();
			type = Buff.Type.Exposure;
		}

		/// <summary>
		/// 랜덤한 카드 조건을 생성한다
		/// </summary>
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

		/// <summary>
		/// 카드에 피폭버프 적용
		/// </summary>
		public void SetExposure(Card card)
		{
			card.isExposure = true;
			card.condition = GetRandomCondition();
			exposureCards.Add(card);
		}
		
		/// <summary>
		/// 턴 시작 시 피폭이 있다면 손패의 랜덤한 카드에 적용한다
		/// </summary>
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

		/// <summary>
		/// 드로우시 남은 피폭버프가 있다면 드로우 한 카드에 적용한다
		/// </summary>
		public override void OnDrawCard(ref Card card)
		{
			if (count == 0) return;
			if (count - player.hand.Count > 0)
			{
				SetExposure(card);
			}
		}

		/// <summary>
		/// 사용한 카드가 피폭된 카드라면 피폭버프스택 1감소
		/// </summary>
		public override bool BeforeUseCard(ref Card card)
		{
			if (count > 0 && card.isExposure)
			{
				count--;
				exposureCards.Remove(card);
			}
			return true;
		}
		
		/// <summary>
		/// 카드 사용 후 피폭버프에 변동이 있을 경우
		/// </summary>
		public override void AfterUseCard(ref Card card, ref CardEffectModule.Result[] results)
		{
			//피폭된 카드 수가 피폭버프스택보다 적고 손패의 개수보다 많을때만 발동 
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