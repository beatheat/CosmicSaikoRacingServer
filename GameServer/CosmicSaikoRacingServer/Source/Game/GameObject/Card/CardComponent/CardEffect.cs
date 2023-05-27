

using CSR.Game.Phase;
using CSR.Game.Player;

namespace CSR.Game.GameObject;

/// <summary>
/// 카드 효과 클래스
/// </summary>
public class CardEffect
{
	public int Count => _cardEffectModules.Count;
	
	private readonly List<CardEffectModule> _cardEffectModules;

	public CardEffect(List<CardEffectModule> cardCardEffectElements)
	{
		this._cardEffectModules = cardCardEffectElements;
	}

	public static CardEffect Nothing()
	{
		EffectModuleManager.TryGet("Nothing", out var nothingEffect, out var type);
		return new CardEffect(new List<CardEffectModule> {new CardEffectModule(nothingEffect, null!, type)});
	}

	public CardEffectModule.Result[] Use(PreheatPhase phase, Card card, GamePlayer player)
	{
		// CardEffectModule.Result[] results = new CardEffectModule.Result[_cardEffectModules.Count];
		List<CardEffectModule.Result> results = new List<CardEffectModule.Result>();
		for (int i = 0; i < _cardEffectModules.Count; i++)
		{
			// 버리기 효과는 무시
			if (_cardEffectModules[i].type == CardEffectModule.Type.Leak)
				continue;

			if (card.Enable)
				results.Add(_cardEffectModules[i].effectModule(phase, card, player, _cardEffectModules[i].parameterList));
		}

		return results.ToArray();
	}

	public CardEffectModule.Result[] UseLeak(PreheatPhase phase, Card card, GamePlayer player)
	{
		CardEffectModule.Result[] results = new CardEffectModule.Result[_cardEffectModules.Count];
		for (int i = 0; i < _cardEffectModules.Count; i++)
		{
			// 버리기 효과가 아닐시 무시
			if (_cardEffectModules[i].type != CardEffectModule.Type.Leak)
				continue;

			if (card.Enable)
				results[i] = _cardEffectModules[i].effectModule(phase, card, player, _cardEffectModules[i].parameterList);
		}

		return results;
	}

	public CardEffectModule.Result Use(int index, PreheatPhase phase, Card card, GamePlayer player)
	{
		if (index < 0 || index > _cardEffectModules.Count)
			index = 0;
		return _cardEffectModules[index].effectModule(phase, card, player, _cardEffectModules[index].parameterList);
		;
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