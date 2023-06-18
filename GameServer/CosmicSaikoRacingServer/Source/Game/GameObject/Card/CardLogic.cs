using CSR.Game;
using CSR.Game.Phase;


namespace CSR.Game.GameObject;

public partial class Card
{
	public Card Clone()
	{
		var card = (Card) this.MemberwiseClone();
		var cloneVariables = new Dictionary<string, Variable>(card.variables.Count, card.variables.Comparer);
		foreach (var entry in card.variables)
		{
			cloneVariables.Add(entry.Key, entry.Value.Clone());
		}

		card.variables = cloneVariables;
		return card;
	}

	/// <summary>
	/// 카드 효과 사용, 버리기 시 버리기 효과 사용
	/// </summary>
	public CardEffect.Result UseEffect(PreheatPhase phase, GamePlayer gamePlayer, bool isDiscard = false)
	{
		UsedCount++;
		var results = isDiscard ? Effect.UseLeak(phase, this, gamePlayer) : Effect.Use(phase, this, gamePlayer);
		return results;
	}
}