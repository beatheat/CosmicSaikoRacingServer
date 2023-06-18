using CSR.Game.GameObject;

namespace CSR.Game;

public static class MaintainLogic
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
	private static readonly int[,] _cardRankPercentage =
	{
		{100, 0, 0}, //0레벨
		{100, 0, 0}, {80, 100, 0}, {60, 90, 100}, {40, 80, 100}, {20, 70, 100}
	};

	//레벨별 제거 카드수 0~5레벨
	private static readonly int[] _cardRemoveCount = {0, 4, 4, 5, 5, 6};

	//레벨별 경험치 최대치 0~5레벨 
	//TODO: 마스터 데이터에 추가하기
	private static readonly int[] _expLimit = {0, 1, 3, 7, 10};
	

	public static void InitMaintain(this GamePlayer player)
	{
		player.Maintain.ShopCards = new List<Card>();
		player.Maintain.RemoveCards = new List<Card>();

		player.Maintain.Coin = INITIAL_COIN_COUNT;
		player.Maintain.TurnCoinCount = INITIAL_COIN_COUNT;
		player.Exp = 0;
		player.ExpLimit = _expLimit[player.Level];
	}


	public static void MaintainOnTurnStart(this GamePlayer player)
	{
		SetRandomStoreCards(player);
		SetRandomRemoveCards(player);
	}

	public static void MaintainOnTurnEnd(this GamePlayer player)
	{
		player.Maintain.TurnCoinCount++;
		player.Maintain.TurnCoinCount = player.Maintain.TurnCoinCount  < MAX_COIN_COUNT ? player.Maintain.TurnCoinCount  : MAX_COIN_COUNT;
		player.Maintain.Coin = player.Maintain.TurnCoinCount;
	}

	/// <summary>
	/// 구매 상점 표시
	/// </summary>
	public static void SetRandomStoreCards(this GamePlayer player)
	{
		player.Maintain.ShopCards.Clear();

		//레벨에 맞는 카드군 생성
		Random random = new Random();
		player.Maintain.ShopCards = new List<Card>();
		for (int i = 0; i < player.Level + 2; i++)
		{
			int randomNumber = random.Next(100);
			//rank 1 카드 
			if (randomNumber < _cardRankPercentage[player.Level, 0])
			{
				player.Maintain.ShopCards.Add(CardManager.GetRandomCardWithCondition(1, 1));
			}
			//rank 2 카드
			else if (randomNumber < _cardRankPercentage[player.Level, 1])
			{
				player.Maintain.ShopCards.Add(CardManager.GetRandomCardWithCondition(2, 2));
			}
			//rank 3 카드
			else if (randomNumber < _cardRankPercentage[player.Level, 2])
			{
				player.Maintain.ShopCards.Add(CardManager.GetRandomCardWithCondition(3, 3));
			}
		}
	}

	/// <summary>
	/// 제거 상점 표시
	/// </summary>
	public static void SetRandomRemoveCards(this GamePlayer player)
	{
		player.Maintain.RemoveCards.Clear();
		//덱에서 카드 몇개 봅음
		Util.DistributeOnList(player.Card.Deck, _cardRemoveCount[player.Level], out var removeCards);
		player.Maintain.RemoveCards = removeCards;
	}


	/// <summary>
	/// 구매상점 리롤
	/// </summary>
	public static ErrorCode RerollShop(this GamePlayer player)
	{
		if (player.Maintain.Coin < COIN_REROLL)
			return ErrorCode.COIN_NOT_ENOUGH;
		player.Maintain.Coin--;
		SetRandomStoreCards(player);
		return ErrorCode.NONE;
	}

	/// <summary>
	/// 제거 상점 리롤
	/// </summary>
	public static ErrorCode RerollRemoveCard(this GamePlayer player)
	{
		if (player.Maintain.Coin < COIN_REROLL)
			return ErrorCode.COIN_NOT_ENOUGH;
		player.Maintain.Coin--;
		SetRandomRemoveCards(player);
		return ErrorCode.NONE;
	}

	/// <summary>
	/// 상점에서 카드 구매
	/// </summary>
	public static ErrorCode BuyCard(this GamePlayer player, int buyIndex, out Card boughtCard)
	{
		boughtCard = null!;

		if (player.Maintain.Coin < COIN_BUY_CARD)
		{
			return ErrorCode.COIN_NOT_ENOUGH;
		}

		if (buyIndex < 0 || buyIndex >= player.Maintain.ShopCards.Count)
		{
			return ErrorCode.WRONG_INDEX;
		}

		player.Maintain.Coin -= 2;
		boughtCard = player.Maintain.ShopCards[buyIndex];
		player.Maintain.ShopCards.RemoveAt(buyIndex);

		return ErrorCode.NONE;
	}


	/// <summary>
	/// 상점에서 카드 제거
	/// </summary>
	public static ErrorCode RemoveCard(this GamePlayer player, int index, out Card removedCard)
	{
		removedCard = null!;

		if (player.Maintain.Coin < COIN_REMOVE_CARD)
			return ErrorCode.COIN_NOT_ENOUGH;

		if (index < 0 && index >= player.Maintain.RemoveCards.Count)
			return ErrorCode.WRONG_INDEX;

		player.Maintain.Coin -= 2;

		Card selectedCard = player.Maintain.RemoveCards[index];
		int selectedIndex = player.Card.Deck.FindIndex(x => x == selectedCard);
		player.RemoveCardFromDeck(selectedIndex);
		player.Maintain.RemoveCards.RemoveAt(index);

		removedCard = selectedCard;

		return ErrorCode.NONE;
	}

	/// <summary>
	/// 상점에서 경험치 구매
	/// </summary>
	public static ErrorCode BuyExp(this GamePlayer player)
	{
		if (player.Maintain.Coin < COIN_BUY_EXP)
			return ErrorCode.COIN_NOT_ENOUGH;
		if (player.Level >= _expLimit.Length)
			return ErrorCode.MAX_LEVEL;

		player.Maintain.Coin -= 3;
		player.Exp++;

		if (player.Exp >= _expLimit[player.Level])
		{
			player.Level++;
			player.Exp = 0;
			player.ExpLimit = _expLimit[player.Level];

			switch (player.Level)
			{
				case 2 or 4:
					player.AddAvailableRerollCount(2);
					break;
				case 3 or 5:
					player.AddReelCount(1);
					break;
			}
		}

		return ErrorCode.NONE;
	}


}