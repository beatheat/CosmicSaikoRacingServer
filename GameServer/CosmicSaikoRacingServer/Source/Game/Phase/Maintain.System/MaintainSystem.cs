
namespace CSRServer.Game
{
	public class MaintainSystem
	{
		public enum ErrorCode
		{
			NONE,
			COIN_NOT_ENOUGH,
			WRONG_INDEX,
			
			MAX_LEVEL
		}
		
		//카드 구매 비용
		private const int COIN_BUY_CARD = 2;
		//상점 리롤 비용
		private const int COIN_REROLL = 1;
		//카드 삭제 비용
		private const int COIN_REMOVE_CARD = 2;
		//경험치 구매 비용
		private const int COIN_BUY_EXP = 3;
		
		//최초 코인 카운트 , PreheatStart호출때 1을 더하기 떄문에 1적게 시작한다
		private const int INITIAL_COIN_COUNT = 3; 
		//최대 코인 카운트
		private const int MAX_COIN_COUNT = 10;
		//최대 레벨
		private const int MAX_LEVEL = 5;

		//레벨별 카드 랭크 등장 확률 0~5레벨
		private static readonly int[,] _cardRankPercentage = {
			{100, 0, 0}, //0레벨
			{100, 0 ,0},
			{80, 100 ,0},
			{60, 90, 100},
			{40, 80, 100},
			{20, 70, 100}
		};
		//레벨별 제거 카드수 0~5레벨
		private static readonly int[] _cardRemoveCount = {0, 4, 4, 5, 5, 6}; 
		//레벨별 경험치 최대치 0~5레벨 
		//TODO: 마스터 데이터에 추가하기
		private static readonly int[] _expLimit = {0, 1, 3, 7, 10};
		
		private readonly GamePlayer _player;
		
		//상점 카드
		private List<Card> _shopCards;
		//제거 카드
		private List<Card> _removeCards;
		
		
		//코인
		public int coin { private set; get; }
		//경험치
		public int exp { private set; get; }
		//레벨
		public int level { private set; get; }
		//턴 당 시작 코인 
		public int turnCoinCount { private set; get; }

		//상점 카드
		public List<Card> shopCards => _shopCards;
		//제거 카드
		public List<Card> removeCards => _removeCards;
		
		/// <summary>
		/// 상점 초기화 count는 플레이어 수
		/// </summary>
		public MaintainSystem(GamePlayer player)
		{
			_player = player;
			_shopCards = new List<Card>();
			_removeCards = new List<Card>();

			exp = 0;
			level = 1;

			coin = INITIAL_COIN_COUNT;
			turnCoinCount = INITIAL_COIN_COUNT;
		}


		public void TurnStart()
		{
			SetRandomStoreCards();
			SetRandomRemoveCards();
		}

		public void TurnEnd()
		{
			turnCoinCount++;
			turnCoinCount = turnCoinCount < MAX_COIN_COUNT ? turnCoinCount : MAX_COIN_COUNT;
			coin = turnCoinCount;
		}
		
		/// <summary>
		/// 구매 상점 표시
		/// </summary>
		public void SetRandomStoreCards()
		{
			_shopCards.Clear();

			//레벨에 맞는 카드군 생성
	        Random random = new Random();
	        _shopCards = new List<Card>();
	        for (int i = 0; i < level + 2; i++)
	        {
		        int randomNumber = random.Next(100);
		        //rank 1 카드 
		        if (randomNumber < _cardRankPercentage[level, 0])
		        {
			        _shopCards.Add(CardManager.GetRandomCardWithCondition(1, 1));
		        }
		        //rank 2 카드
		        else if (randomNumber < _cardRankPercentage[level, 1])
		        {
			        _shopCards.Add(CardManager.GetRandomCardWithCondition(2, 2));
		        }
		        //rank 3 카드
		        else if (randomNumber < _cardRankPercentage[level, 2])
		        {
			        _shopCards.Add(CardManager.GetRandomCardWithCondition(3, 3));
		        }
	        }
		}
		
		/// <summary>
		/// 제거 상점 표시
		/// </summary>
		public void SetRandomRemoveCards()
		{
			_removeCards.Clear();
			//덱에서 카드 몇개 봅음
			Util.DistributeOnList(_player.cardSystem.deck, _cardRemoveCount[level], out _removeCards);
		}


		/// <summary>
		/// 구매상점 리롤
		/// </summary>
		public ErrorCode RerollShop()
		{
			if (coin < COIN_REROLL)
				return ErrorCode.COIN_NOT_ENOUGH;
			coin--;
			SetRandomStoreCards();
			return ErrorCode.NONE;
		}
		
		/// <summary>
		/// 제거 상점 리롤
		/// </summary>
		public ErrorCode RerollRemoveCard()
		{
			if (coin < COIN_REROLL)
				return ErrorCode.COIN_NOT_ENOUGH;
			coin--;
			SetRandomRemoveCards();
			return ErrorCode.NONE;
		}
		
		/// <summary>
		/// 상점에서 카드 구매
		/// </summary>
		public ErrorCode BuyCard(int buyIndex, out Card boughtCard)
		{
			boughtCard = null!;
			
			if (coin < COIN_BUY_CARD)
			{
				return ErrorCode.COIN_NOT_ENOUGH;
			}
			
			if (buyIndex < 0 || buyIndex >= _shopCards.Count)
			{
				return ErrorCode.WRONG_INDEX;
			}

			coin -= 2;
			boughtCard = _shopCards[buyIndex];
			_shopCards.RemoveAt(buyIndex);
			
			return ErrorCode.NONE;
		}

		
		/// <summary>
		/// 상점에서 카드 제거
		/// </summary>
		public ErrorCode RemoveCard(int index, out Card removedCard)
		{
			removedCard = null!;
			
			if(coin < COIN_REMOVE_CARD)
				return ErrorCode.COIN_NOT_ENOUGH;
			
			if (index < 0 && index >= removeCards.Count)
				return ErrorCode.WRONG_INDEX;

			coin -= 2;
			
			Card selectedCard = removeCards[index];
			int selectedIndex = _player.cardSystem.deck.FindIndex(x => x == selectedCard);
			_player.cardSystem.RemoveCardFromDeck(selectedIndex);
			this._removeCards.RemoveAt(index);
			
			removedCard = selectedCard;

			return ErrorCode.NONE;
		}
		
		/// <summary>
		/// 상점에서 경험치 구매
		/// </summary>
		public ErrorCode BuyExp()
		{
			if (coin < COIN_BUY_EXP)
				return ErrorCode.COIN_NOT_ENOUGH;
			if (level >= _expLimit.Length) 
				return ErrorCode.MAX_LEVEL;
			
			coin -= 3;
			exp++;
			
			if (exp >= _expLimit[level])
			{
				level++;
				exp = 0;

				switch (level)
				{
					case 2 or 4:
						_player.resourceSystem.AddAvailableRerollCount(2);
						break;
					case 3 or 5:
						_player.resourceSystem.AddReelCount(1);
						break;
				}
			}
			
			return ErrorCode.NONE;
		}
		
		
	}
}