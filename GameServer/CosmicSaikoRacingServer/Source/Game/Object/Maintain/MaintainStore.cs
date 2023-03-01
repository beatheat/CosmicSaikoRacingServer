
namespace CSRServer.Game
{
	internal class MaintainStore
	{

		private readonly List<List<Card>> playerStoreCards;
		private readonly List<List<Card>> playerRemoveCards;
		private readonly int[] expLimit = {0, 1, 3, 7, 10};
		private readonly int[,] cardRankPercentage = {
			{100, 0 ,0},
			{80, 20 ,0},
			{60, 30, 10},
			{40, 40, 20},
			{20, 50, 30}
		};
		private readonly int[] cardRemoveCount = {4, 4, 5, 5, 6};


		public MaintainStore(int count)
		{
			playerStoreCards = new List<List<Card>>(count);
			playerRemoveCards = new List<List<Card>>(count);
		}
		
		public bool ShowRandomCards(GamePlayer player, out List<Card> cards)
		{
			cards = new List<Card>();
			if (player.index < 0 || player.index >= playerStoreCards.Count)
				return false;
			
	        //레벨에 맞는 카드군 생성
	        Random random = new Random();
	        cards = new List<Card>();
	        for (int i = 0; i < player.level + 2; i++)
	        {
		        int randomNumber = random.Next(100);
		        //rank 1 카드 
		        if (randomNumber < cardRankPercentage[player.level - 1, 0])
		        {
			        cards.Add(CardManager.GetRandomCardWithCondition(1, 1));
		        }
		        //rank 2 카드
		        else if (randomNumber < cardRankPercentage[player.level - 1, 1])
		        {
			        cards.Add(CardManager.GetRandomCardWithCondition(2, 2));
		        }
		        //rank 3 카드
		        else if (randomNumber < cardRankPercentage[player.level - 1, 2])
		        {
			        cards.Add(CardManager.GetRandomCardWithCondition(3, 3));
		        }
	        }

	        playerStoreCards[player.index] = cards;
	        return true;
		}
		
		public bool ShowRandomRemoveCards(GamePlayer player, out List<Card> cards)
		{
			cards = new List<Card>();
			if (player.index < 0 || player.index >= playerStoreCards.Count)
				return false;
			
			//덱에서 카드 몇개 봅음
			Util.DistributeOnList(player.deck, cardRemoveCount[player.level - 1], out cards);
			playerRemoveCards[player.index] = cards;
			return true;
		}

		public bool RemoveCard(GamePlayer player, int index, out List<Card> removeCards)
		{
			removeCards = new List<Card>();
			
			if (index < 0 && index >= playerRemoveCards[player.index].Count)
				return false;
			
			
			Card selectedCard = playerRemoveCards[player.index][index];
			int selectedIndex = player.deck.FindIndex(x => x == selectedCard);
			player.RemoveCardFromDeck(selectedIndex);
			playerRemoveCards[player.index].RemoveAt(index);
			removeCards = playerRemoveCards[player.index];

			return true;
		}
		

		public bool RerollStore(GamePlayer player, out List<Card>? cards)
		{
			cards = null;
			if (player.coin < 1)
				return false;
			player.coin--;
			return ShowRandomCards(player, out cards);
		}
		

		public bool BuyCard(GamePlayer player, int buyIndex,out List<Card> storeCards, out Card? buyCard)
		{
			buyCard = null;
			storeCards = new List<Card>();
			if (player.coin < 2)
				return false;
			
			if (player.index < 0 || player.index >= playerStoreCards.Count)
				return false;
			

			if (buyIndex < 0 || buyIndex >= playerStoreCards.Count)
				return false;

			player.coin -= 2;
			storeCards = playerStoreCards[player.index];
			buyCard = storeCards[buyIndex];
			playerStoreCards.RemoveAt(buyIndex);
			
			return true;
		}

		public bool BuyExp(GamePlayer player)
		{
			if (player.coin < 3)
				return false;
			player.coin -= 3;
			if (player.level < expLimit.Length)
			{
				player.exp++;
				if (player.exp >= expLimit[player.level])
				{
					player.level++;
					player.exp = 0;
				}
			}
			return true;
		}
		
		
	}
}