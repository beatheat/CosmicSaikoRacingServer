using System.Text.Json.Serialization;

namespace CSRServer.Game
{
	public class CardSystem
	{
		//최초 카드 드로우 카운트
		private const int INITIAL_DRAW_COUNT = 5;
		
		private int _drawCount;
		private readonly GamePlayer _player;

		//덱, 핸드, 묘지
		public List<Card> hand { get; }
		public List<Card> deck { get; }
		public List<Card> usedCard { get; private set; }
		public List<Card> unusedCard { get; private set; }
		
		public List<Card> turnUsedCard { get; }

		public CardSystem(GamePlayer player)
		{
			_player = player;

			deck = new List<Card>();
			hand = new List<Card>();
			usedCard = new List<Card>();
			unusedCard = new List<Card>();

			turnUsedCard = new List<Card>();
			
			_drawCount = INITIAL_DRAW_COUNT;
		}

		/// <summary>
		/// 턴 시작 로직
		/// </summary>
		public void TurnStart()
		{
			DrawCard(_drawCount);
		}
		
		/// <summary>
		/// 턴 종료 로직
		/// </summary>
		public void TurnEnd()
		{
			//손패 전부 버리기
			while (hand.Count > 0)
			{
				DiscardCard(0);
			}
			//이번턴에 사용한 카드 리스트 초기화
			turnUsedCard.Clear();
		}
		
		/// <summary>
		/// 카드가 리소스릴 조건에 맞는지 확인
		/// </summary>
		public bool IsCardEnable(int index)
		{
			if (index >= hand.Count || index < 0)
				return false;
			Card card = hand[index];
			return _player.resourceSystem.CheckCardUsable(card);
		}

		/// <summary>
		/// 패에서 카드를 사용한다
		/// </summary>
		public bool UseCard(int index, out CardEffectModule.Result[] result)
		{
			if (index >= hand.Count || index < 0)
			{
				result = null!;
				return false;
			}
			Card card = hand[index];
			result = Array.Empty<CardEffectModule.Result>();
			//패에서 카드를 삭제
			hand.RemoveAt(index);
			// 카드 사용전 버프 적용
			if (_player.buffSystem.BeforeUseCard(ref card, ref result) == false)
			{
				//제거한 카드를 묘지로 보낸다
				MoveCardToGrave(card);
				return false;
			}
			//카드 사용
			result = card.UseEffect(_player);
			//카드 사용가능 복구, 고장시 false로 바뀌어 복구한다.
			card.enable = true;
			//카드 사용후 버프 적용
			_player.buffSystem.AfterUseCard(ref card);
			//제거한 카드를 묘지로 보낸다
			MoveCardToGrave(card);
			//이번턴에 사용한 카드 리스트에 추가
			turnUsedCard.Add(card);
			return true;
		}

		
		/// <summary>
        /// 덱에서 카드를 한장 뽑는다
        /// </summary>
        public List<Card>? DrawCard(int count)
        {
            if (count < 0)
                return null;
            int remainCount = 0;
            
            List<Card> cards = new List<Card>();
            
            if (count > unusedCard.Count)
            {
                remainCount = count - unusedCard.Count;
                count = unusedCard.Count;
            }
            
            for (int i = 0; i < count; i++)
            {
                //덱에서 랜덤한 카드를 선택하여 패에 추가한다
                Random rand = new Random();
                int drawIndex = rand.Next(unusedCard.Count);
                Card card = unusedCard[drawIndex];
                //카드를 뽑았을 때 적용된 버프 제거
                card.isExposure = false;
                card.isMimesis = false; 
                
                hand.Add(unusedCard[drawIndex]);
                cards.Add(unusedCard[drawIndex]);
                unusedCard.RemoveAt(drawIndex);
                
                //카드를 뽑은후 버프 실행
                _player.buffSystem.OnDrawCard(ref card);
            }

            //드로우할 카드 수가 남은 덱의 수보다 많으면 묘지를 섞고 다시 드로우한다
            if (remainCount > 0)
            {
                //묘지와 덱을 스왑한다. 이때 덱은 항상 0장이다
                (usedCard, unusedCard) = (unusedCard, usedCard);
                if (remainCount > unusedCard.Count)
                    remainCount = unusedCard.Count;

                for (int i = 0; i < remainCount; i++)
                {
                    Random rand = new Random();
                    int drawIndex = rand.Next(unusedCard.Count);
                    Card card = unusedCard[drawIndex];
                    //카드를 뽑았을 때 적용된 버프 제거
                    card.isExposure = false;
                    card.isMimesis = false; 

                    hand.Add(unusedCard[drawIndex]);
                    cards.Add(unusedCard[drawIndex]);
                    unusedCard.RemoveAt(drawIndex);
                    //카드를 뽑은후 버프 실행
                    _player.buffSystem.OnDrawCard(ref card);
                }
            }
            
            return cards;
        }
		       
		               
        /// <summary>
        /// 카드를 패에 추가한다
        /// </summary>
        public void AddCardToHand(params Card[] card)
        {
            foreach (var c in card)
            {
                deck.Add(c);
                hand.Add(c);
            }
        }

        /// <summary>
        /// 카드를 덱에 추가한다
        /// </summary>
        public void AddCardToDeck(params Card[] card)
        {
            foreach (var c in card)
            {
                deck.Add(c);
                unusedCard.Add(c);
            }
        }

        /// <summary>
        /// 덱에서 카드를 제거한다
        /// </summary>
        public bool RemoveCardFromDeck(int index)
        {
            if (index < 0 || index >= deck.Count)
                return false;
            Card card = deck[index];
            deck.RemoveAt(index);
            unusedCard.Remove(card);
            usedCard.Remove(card);
            return true;
        }

        /// <summary>
        /// 카드를 묘지로 옮긴다.
        /// </summary>
        public void MoveCardToGrave(Card card)
        {
            //제거한 카드를 묘지로 보낸다
            if (card.death)
            {
                deck.Remove(card);
                unusedCard.Remove(card);
                usedCard.Remove(card);
            }
            else
                usedCard.Add(card);
        }

        /// <summary>
        /// 패에서 카드를 제거한다
        /// </summary>
        public void DiscardCard(int index)
        {
            if (index >= hand.Count || index < 0)
            {
                return;
            }
            Card card = hand[index];

            //패에서 삭제
            hand.RemoveAt(index);
            //제거한 카드를 묘지로 보낸다
            MoveCardToGrave(card);
        }
        
        /// <summary>
        /// 패에서 카드제거와는 다른 "버리기"를 수행한다 
        /// </summary>
        public CardEffectModule.Result[] ThrowCard(int index)
        {
            if (index >= hand.Count || index < 0)
            {
                return Array.Empty<CardEffectModule.Result>();
            }
            Card card = hand[index];
            DiscardCard(index);
            //버리기 후 버프적용
            _player.buffSystem.OnThrowCard(card);
            return card.UseEffect(_player, isDiscard: true);
        }
	}
}