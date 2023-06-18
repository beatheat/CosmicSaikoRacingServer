using CSR.Game.GameObject.CardEffectModuleImplementation;
using CSR.Game.Phase;

namespace CSR.Game.GameObject;

/// <summary>
/// 카드 효과 클래스
/// </summary>
public partial class CardEffect
{
	public int Count => _cardEffectModules.Count;
	
	private readonly List<CardEffectModule> _cardEffectModules;

	public CardEffect(List<CardEffectModule> cardCardEffectElements)
	{
		this._cardEffectModules = cardCardEffectElements;
	}

	public static CardEffect Nothing()
	{
		return new CardEffect(new List<CardEffectModule> {new Nothing(null!)});
	}

	public Result Use(PreheatPhase phase, Card card, GamePlayer player)
	{
		List<CardEffectModule.Result> results = new List<CardEffectModule.Result>();
		for (int i = 0; i < _cardEffectModules.Count; i++)
		{
			// 버리기 효과는 무시
			if (_cardEffectModules[i].type == CardEffectModule.Type.Leak)
				continue;

			if (card.Enable)
				results.Add(_cardEffectModules[i].Activate(phase, card, player));
		}

		return new Result(results);
	}
	
	public Result Use(int index, PreheatPhase phase, Card card, GamePlayer player)
	{
		if (index < 0 || index > _cardEffectModules.Count)
			index = 0;
		return new Result(_cardEffectModules[index].Activate(phase, card, player));
	}

	public Result UseLeak(PreheatPhase phase, Card card, GamePlayer player)
	{
		List<CardEffectModule.Result> results = new List<CardEffectModule.Result>();
		for (int i = 0; i < _cardEffectModules.Count; i++)
		{
			// 버리기 효과가 아닐시 무시
			if (_cardEffectModules[i].type != CardEffectModule.Type.Leak)
				continue;

			if (card.Enable)
				results.Add(_cardEffectModules[i].Activate(phase, card, player));
		}

		return new Result(results);
	}

	public CardEffectModule.Type[] GetTypes()
	{
		CardEffectModule.Type[] typeList = new CardEffectModule.Type[_cardEffectModules.Count];
		for (int i = 0; i < _cardEffectModules.Count; i++)
		{
			typeList[i] = _cardEffectModules[i].type;
		}

		return typeList;
	}

}