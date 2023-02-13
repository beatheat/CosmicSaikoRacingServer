
namespace CSRServer
{
	internal class MaintainStore
	{

		private readonly List<List<Card>> playerStoreCards;
		
		public MaintainStore(int count)
		{
			playerStoreCards = new List<List<Card>>(count);
		}
		
		public List<Card> ShowRandomCards(int playerIndex, int level)
		{
			if (playerIndex < 0 || playerIndex >= playerStoreCards.Count)
				return new List<Card>();
			
	        List<Card> cards = new List<Card>();
	        //레벨에 맞는 카드군 생성
	        
	        playerStoreCards[playerIndex] = cards;
	        return cards;
        }

		public Card? BuyCard(int playerIndex, int buyIndex)
		{
			if (playerIndex < 0 || playerIndex >= playerStoreCards.Count)
				return null;
			
			var storeCards = playerStoreCards[playerIndex];

			if (buyIndex < 0 || buyIndex >= playerStoreCards.Count)
				return null;

			var buyCard = storeCards[buyIndex];
			playerStoreCards.RemoveAt(buyIndex);
			return buyCard;
		}
	}
}