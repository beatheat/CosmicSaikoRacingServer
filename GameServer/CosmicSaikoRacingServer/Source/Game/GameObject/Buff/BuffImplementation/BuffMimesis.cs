using CSR.Game.Phase;
using CSR.Game.Player;
using ProtoBuf;

namespace CSR.Game.GameObject;


[ProtoContract]
internal class BuffMimesis : Buff
{
	private readonly List<Card> _mimesisCards;
	private bool _turnStart;

	public BuffMimesis(PreheatPhase phase, GamePlayer player) : base(phase, player)
	{
		Type = BuffType.Mimesis;
		_mimesisCards = new List<Card>();
		_turnStart = false;
	}

	/// <summary>
	/// 의태 버프를 적용한다
	/// </summary>
	private void SetMimesis(params Card[] cards)
	{
		foreach (var card in cards)
		{
			card.IsMimesis = true;
			card.Condition = new CardCondition(0);
			_mimesisCards.Add(card);
		}
	}

	/// <summary>
	/// 턴 시작 시 의태버프가 존재하면 적용한다
	/// </summary>
	public override void OnPreheatStart()
	{
		_mimesisCards.Clear();
		
		if (Count == 0) return;


		//최대 패 수 만큼 의태
		int mimesisCount = Count > player.Card.Hand.Count ? player.Card.Hand.Count : Count;


		//랜덤 패에 부여
		Util.DistributeOnList(player.Card.Hand, mimesisCount, out var selectedCards);
		foreach (var card in selectedCards)
		{
			SetMimesis(card);
		}

		_turnStart = true;
	}

	/// <summary>
	/// 카드 드로우 시 남은 의태버프가 있으면 적용한다
	/// </summary>
	public override void OnDrawCard(Card card)
	{
		if (Count == 0) return;
		if (_turnStart && Count - player.Card.Hand.Count > 0)
		{
			SetMimesis(card);
		}
	}

	/// <summary>
	/// 카드 사용 시 의태한 카드면 의태버프스택 1 감소한다
	/// </summary>
	public override bool BeforeUseCard(Card card, ref CardEffectModule.Result[] results)
	{
		if (Count > 0 && card.IsMimesis)
		{
			Count--;
			_mimesisCards.Remove(card);
		}

		return true;
	}

	/// <summary>
	/// 카드 사용 후 의태 버프에 변동이 있으면 적용한다
	/// </summary>
	public override void AfterUseCard(Card card)
	{
		//의태한 카드 수가 의태버프스택보다 적을 때 발동
		if (_mimesisCards.Count < Count)
		{
			int mimesisCount = Count - _mimesisCards.Count;
			var nonMimesisHand = player.Card.Hand.FindAll(x => x.IsMimesis == false);
			Util.DistributeOnList(nonMimesisHand, mimesisCount, out var selectedCards);
			SetMimesis(selectedCards.ToArray());
		}
	}

	public override void OnPreheatEnd()
	{
		Release();
		_turnStart = false;
	}

	public override int Release()
	{
		foreach (var card in _mimesisCards)
		{
			card.IsMimesis = false;
			card.Condition = CardManager.GetCard(card.Id).Condition;
		}

		_mimesisCards.Clear();
		return base.Release();
	}
}