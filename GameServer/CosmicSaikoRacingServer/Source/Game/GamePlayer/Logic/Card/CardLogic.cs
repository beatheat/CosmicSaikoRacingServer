
using CSR.Game.GameObject;
using CSR.Game.Phase;

namespace CSR.Game.Player;

public static class CardLogic
{
	//최초 카드 드로우 카운트
	private const int INITIAL_DRAW_COUNT = 5;


	public static void InitCard(this GamePlayer player)
	{
		player.Card.Deck = new List<Card>();
		player.Card.Hand = new List<Card>();
		player.Card.Used = new List<Card>();
		player.Card.Unused = new List<Card>();
		player.Card.TurnUsed = new List<Card>();
		player.Card.DrawCount = INITIAL_DRAW_COUNT;
	}

	/// <summary>
	/// 턴 시작 로직
	/// </summary>
	public static void CardOnTurnStart(this GamePlayer player)
	{
		player.DrawCard(player.Card.DrawCount);
	}

	/// <summary>
	/// 턴 종료 로직
	/// </summary>
	public static void CardOnTurnEnd(this GamePlayer player)
	{
		//손패 전부 버리기
		while (player.Card.Hand.Count > 0)
		{
			player.DiscardCard(0);
		}

		//이번턴에 사용한 카드 리스트 초기화
		player.Card.TurnUsed.Clear();
	}

	/// <summary>
	/// 카드가 리소스릴 조건에 맞는지 확인
	/// </summary>
	public static bool ValidateHandIndex(this GamePlayer player, int index)
	{
		if (index >= player.Card.Hand.Count || index < 0)
			return false;
		return true;
	}
	
	/// <summary>
	/// 카드가 리소스릴 조건에 맞는지 확인
	/// </summary>
	public static bool IsCardEnable(this GamePlayer player, int index)
	{
		if (index >= player.Card.Hand.Count || index < 0)
			return false;
		Card card = player.Card.Hand[index];
		return player.CheckCardUsable(card);
	}

	/// <summary>
	/// 패에서 카드를 사용한다
	/// </summary>
	public static bool UseCard(this GamePlayer player, int index, PreheatPhase phase, out GameObject.CardEffectModule.Result[] result)
	{
		Card card = player.Card.Hand[index];
		result = Array.Empty<GameObject.CardEffectModule.Result>();
		//패에서 카드를 삭제
		player.Card.Hand.RemoveAt(index);
		// 카드 사용전 버프 적용
		if (player.BuffBeforeUseCard(card, ref result) == false)
		{
			//제거한 카드를 묘지로 보낸다
			MoveCardToGrave(player, card);
			return false;
		}

		//카드 사용
		result = card.UseEffect(phase, player);
		//카드 사용가능 복구, 고장시 false로 바뀌어 복구한다.
		card.Enable = true;
		//카드 사용후 버프 적용
		player.BuffAfterUseCard(card);
		//제거한 카드를 묘지로 보낸다
		player.MoveCardToGrave(card);
		//이번턴에 사용한 카드 리스트에 추가
		player.Card.TurnUsed.Add(card);
		return true;
	}


	/// <summary>
	/// 덱에서 카드를 한장 뽑는다
	/// </summary>
	public static List<Card>? DrawCard(this GamePlayer player, int count)
	{
		if (count < 0)
			return null;
		int remainCount = 0;

		List<Card> cards = new List<Card>();

		if (count > player.Card.Unused.Count)
		{
			remainCount = count - player.Card.Unused.Count;
			count = player.Card.Unused.Count;
		}

		for (int i = 0; i < count; i++)
		{
			//덱에서 랜덤한 카드를 선택하여 패에 추가한다
			Random rand = new Random();
			int drawIndex = rand.Next(player.Card.Unused.Count);
			Card card = player.Card.Unused[drawIndex];
			//카드를 뽑았을 때 적용된 버프 제거
			card.IsExposure = false;
			card.IsMimesis = false;

			player.Card.Hand.Add(player.Card.Unused[drawIndex]);
			cards.Add(player.Card.Unused[drawIndex]);
			player.Card.Unused.RemoveAt(drawIndex);

			//카드를 뽑은후 버프 실행
			player.BuffOnDrawCard(card);
		}

		//드로우할 카드 수가 남은 덱의 수보다 많으면 묘지를 섞고 다시 드로우한다
		if (remainCount > 0)
		{
			//묘지와 덱을 스왑한다. 이때 덱은 항상 0장이다
			(player.Card.Used, player.Card.Unused) = (player.Card.Unused, player.Card.Used);
			if (remainCount > player.Card.Unused.Count)
				remainCount = player.Card.Unused.Count;

			for (int i = 0; i < remainCount; i++)
			{
				Random rand = new Random();
				int drawIndex = rand.Next(player.Card.Unused.Count);
				Card card = player.Card.Unused[drawIndex];
				//카드를 뽑았을 때 적용된 버프 제거
				card.IsExposure = false;
				card.IsMimesis = false;

				player.Card.Hand.Add(player.Card.Unused[drawIndex]);
				cards.Add(player.Card.Unused[drawIndex]);
				player.Card.Unused.RemoveAt(drawIndex);
				//카드를 뽑은후 버프 실행
				player.BuffOnDrawCard(card);
			}
		}

		return cards;
	}


	/// <summary>
	/// 카드를 패에 추가한다
	/// </summary>
	public static void AddCardToHand(this GamePlayer player, params Card[] card)
	{
		foreach (var c in card)
		{
			player.Card.Deck.Add(c);
			player.Card.Hand.Add(c);
		}
	}

	/// <summary>
	/// 카드를 덱에 추가한다
	/// </summary>
	public static void AddCardToDeck(this GamePlayer player, params Card[] card)
	{
		foreach (var c in card)
		{
			player.Card.Deck.Add(c);
			player.Card.Unused.Add(c);
		}
	}

	/// <summary>
	/// 덱에서 카드를 제거한다
	/// </summary>
	public static bool RemoveCardFromDeck(this GamePlayer player, int index)
	{
		if (index < 0 || index >= player.Card.Deck.Count)
			return false;
		Card card = player.Card.Deck[index];
		player.Card.Deck.RemoveAt(index);
		player.Card.Unused.Remove(card);
		player.Card.Used.Remove(card);
		return true;
	}

	/// <summary>
	/// 카드를 묘지로 옮긴다.
	/// </summary>
	public static void MoveCardToGrave(this GamePlayer player, Card card)
	{
		//제거한 카드를 묘지로 보낸다
		if (card.Death)
		{
			player.Card.Deck.Remove(card);
			player.Card.Unused.Remove(card);
			player.Card.Used.Remove(card);
		}
		else
			player.Card.Used.Add(card);
	}

	/// <summary>
	/// 패에서 카드를 제거한다
	/// </summary>
	public static void DiscardCard(this GamePlayer player, int index)
	{
		if (index >= player.Card.Hand.Count || index < 0)
		{
			return;
		}

		Card card = player.Card.Hand[index];

		//패에서 삭제
		player.Card.Hand.RemoveAt(index);
		//제거한 카드를 묘지로 보낸다
		player.MoveCardToGrave(card);
	}

	/// <summary>
	/// 패에서 카드제거와는 다른 "버리기"를 수행한다 
	/// </summary>
	public static GameObject.CardEffectModule.Result[] ThrowCard(this GamePlayer player, int index, PreheatPhase phase)
	{
		if (index >= player.Card.Hand.Count || index < 0)
		{
			return Array.Empty<GameObject.CardEffectModule.Result>();
		}

		Card card = player.Card.Hand[index];
		player.DiscardCard(index);
		//버리기 후 버프적용
		player.BuffOnThrowCard(card);
		return card.UseEffect(phase, player, isDiscard: true);
	}
}