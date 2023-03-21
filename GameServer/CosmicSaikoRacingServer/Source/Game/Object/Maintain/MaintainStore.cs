
namespace CSRServer.Game
{
	internal class MaintainStore
	{
		//카드 구매 비용
		public const int COIN_BUY_CARD = 2;
		//상점 리롤 비용
		public const int COIN_REROLL = 1;
		//카드 삭제 비용
		public const int COIN_REMOVE_CARD = 2;
		//경험치 구매 비용
		public const int COIN_BUY_EXP = 3;

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
		public static readonly int[] cardRemoveCount = {0, 4, 4, 5, 5, 6}; 
		//레벨별 경험치 최대치 0~5레벨
		public static readonly int[] expLimit = {0, 1, 3, 7, 10};


		//상점 카드
		private readonly List<Card>[] _playerStoreCards;
		//제거 카드
		private readonly List<Card>[] _playerRemoveCards;


		/// <summary>
		/// 상점 초기화 count는 플레이어 수
		/// </summary>
		public MaintainStore(int count)
		{
			_playerStoreCards = new List<Card>[count];
			_playerRemoveCards = new List<Card>[count];
		}


		/// <summary>
		/// 구매 상점 표시
		/// </summary>
		public void ShowRandomCards(GamePlayer player, out List<Card> cards)
		{
			cards = new List<Card>();

			//레벨에 맞는 카드군 생성
	        Random random = new Random();
	        cards = new List<Card>();
	        for (int i = 0; i < player.level + 2; i++)
	        {
		        int randomNumber = random.Next(100);
		        //rank 1 카드 
		        if (randomNumber < _cardRankPercentage[player.level, 0])
		        {
			        cards.Add(CardManager.GetRandomCardWithCondition(1, 1));
		        }
		        //rank 2 카드
		        else if (randomNumber < _cardRankPercentage[player.level, 1])
		        {
			        cards.Add(CardManager.GetRandomCardWithCondition(2, 2));
		        }
		        //rank 3 카드
		        else if (randomNumber < _cardRankPercentage[player.level, 2])
		        {
			        cards.Add(CardManager.GetRandomCardWithCondition(3, 3));
		        }
	        }

	        _playerStoreCards[player.index] = cards;
		}
		
		/// <summary>
		/// 제거 상점 표시
		/// </summary>
		public void ShowRandomRemoveCards(GamePlayer player, out List<Card> cards)
		{
			cards = new List<Card>();

			//덱에서 카드 몇개 봅음
			Util.DistributeOnList(player.deck, cardRemoveCount[player.level], out cards);
			_playerRemoveCards[player.index] = cards;
		}


		/// <summary>
		/// 구매상점 리롤
		/// </summary>
		public bool RerollStore(GamePlayer player, out List<Card>? cards)
		{
			cards = null;
			if (player.coin < COIN_REROLL)
				return false;
			player.coin--;
			ShowRandomCards(player, out cards);
			return true;
		}
		
		/// <summary>
		/// 제거 상점 리롤
		/// </summary>
		public bool RerollRemoveCard(GamePlayer player, out List<Card>? cards)
		{
			cards = null;
			if (player.coin < COIN_REROLL)
				return false;
			player.coin--;
			ShowRandomRemoveCards(player, out cards);
			return true;
		}
		
		/// <summary>
		/// 상점에서 카드 구매
		/// </summary>
		public bool BuyCard(GamePlayer player, int buyIndex,out List<Card> storeCards, out Card? buyCard)
		{
			buyCard = null;
			storeCards = _playerStoreCards[player.index];
			
			if (player.coin < COIN_BUY_CARD)
				return false;
			
			if (player.index < 0 || player.index >= storeCards.Count)
				return false;
			

			if (buyIndex < 0 || buyIndex >= storeCards.Count)
				return false;

			player.coin -= 2;
			buyCard = storeCards[buyIndex];
			storeCards.RemoveAt(buyIndex);
			
			return true;
		}

		
		/// <summary>
		/// 상점에서 카드 제거
		/// </summary>
		public bool RemoveCard(GamePlayer player, int index, out List<Card> removeCards)
		{
			removeCards = _playerRemoveCards[player.index];

			if(player.coin < COIN_REMOVE_CARD)
				return false;
			
			if (index < 0 && index >= removeCards.Count)
				return false;

			player.coin -= 2;
			Card selectedCard = removeCards[index];
			int selectedIndex = player.deck.FindIndex(x => x == selectedCard);
			player.RemoveCardFromDeck(selectedIndex);
			_playerRemoveCards[player.index].RemoveAt(index);

			return true;
		}
		
		/// <summary>
		/// 상점에서 경험치 구매
		/// </summary>
		public bool BuyExp(GamePlayer player)
		{
			if (player.coin < COIN_BUY_EXP)
				return false;
			if (player.level >= expLimit.Length) 
				return false;
			
			player.coin -= 3;
			player.exp++;
			
			if (player.exp >= expLimit[player.level])
			{
				player.level++;
				player.exp = 0;
				player.expLimit = expLimit[player.level];

				switch (player.level)
				{
					case 2 or 4:
						player.availableRerollCount++;
						break;
					case 3 or 5:
						player.resourceReelCount++;
						break;
				}
			}
			
			return true;
		}
		
		
	}
}