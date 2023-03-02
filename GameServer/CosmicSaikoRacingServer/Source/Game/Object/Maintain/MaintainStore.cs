
namespace CSRServer.Game
{
	internal class MaintainStore
	{

		private readonly List<Card>[] playerStoreCards;
		private readonly List<Card>[] playerRemoveCards;


		public const int MAX_LEVEL = 5;
		public static readonly int[] expLimit = {0, 1, 3, 7, 10};
		private static readonly int[,] cardRankPercentage = {
			{100, 0, 0}, //0레벨
			{100, 0 ,0},
			{80, 100 ,0},
			{60, 90, 100},
			{40, 80, 100},
			{20, 70, 100}
		};
		private static readonly int[] cardRemoveCount = {0, 4, 4, 5, 5, 6}; // 0레벨은 더미

		public const int COIN_BUY_CARD = 2;
		public const int COIN_REROLL = 1;
		public const int COIN_REMOVE_CARD = 2;
		public const int COIN_BUY_EXP = 3;


		public MaintainStore(int count)
		{
			playerStoreCards = new List<Card>[count];
			playerRemoveCards = new List<Card>[count];
		}


		
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
		        if (randomNumber < cardRankPercentage[player.level, 0])
		        {
			        cards.Add(CardManager.GetRandomCardWithCondition(1, 1));
		        }
		        //rank 2 카드
		        else if (randomNumber < cardRankPercentage[player.level, 1])
		        {
			        cards.Add(CardManager.GetRandomCardWithCondition(2, 2));
		        }
		        //rank 3 카드
		        else if (randomNumber < cardRankPercentage[player.level, 2])
		        {
			        cards.Add(CardManager.GetRandomCardWithCondition(3, 3));
		        }
	        }

	        playerStoreCards[player.index] = cards;
		}
		
		public void ShowRandomRemoveCards(GamePlayer player, out List<Card> cards)
		{
			cards = new List<Card>();

			//덱에서 카드 몇개 봅음
			Util.DistributeOnList(player.deck, cardRemoveCount[player.level], out cards);
			playerRemoveCards[player.index] = cards;
		}


		public bool RerollStore(GamePlayer player, out List<Card>? cards)
		{
			cards = null;
			if (player.coin < COIN_REROLL)
				return false;
			player.coin--;
			ShowRandomCards(player, out cards);
			return true;
		}
		
		
		public bool RerollRemoveCard(GamePlayer player, out List<Card>? cards)
		{
			cards = null;
			if (player.coin < COIN_REROLL)
				return false;
			player.coin--;
			ShowRandomRemoveCards(player, out cards);
			return true;
		}
		

		public bool BuyCard(GamePlayer player, int buyIndex,out List<Card> storeCards, out Card? buyCard)
		{
			buyCard = null;
			storeCards = playerStoreCards[player.index];
			
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

		
		public bool RemoveCard(GamePlayer player, int index, out List<Card> removeCards)
		{
			removeCards = playerRemoveCards[player.index];

			if(player.coin < COIN_REMOVE_CARD)
				return false;
			
			if (index < 0 && index >= removeCards.Count)
				return false;

			player.coin -= 2;
			Card selectedCard = removeCards[index];
			int selectedIndex = player.deck.FindIndex(x => x == selectedCard);
			player.RemoveCardFromDeck(selectedIndex);
			playerRemoveCards[player.index].RemoveAt(index);

			return true;
		}
		
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