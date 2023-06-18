using CSR.Game.Phase;
using ProtoBuf;

#pragma warning disable CS8618

namespace CSR.Game.GameObject.CardEffectModuleImplementation;

public class Combo : CardEffectModule
{
	
	[ProtoContract]
	public class ResultCombo : Result
	{
		[ProtoMember(2)]
		public CardEffect.Result Result { get; set; }
	}

	public Combo(List<Parameter> parameters) : base(parameters, Type.Combo)
	{
	}

	public override Result Activate(PreheatPhase preheatPhase, Card card, GamePlayer player)
	{
		List<int> idList = parameters.Get<List<int>>(0, card, player) ?? new List<int> {0};
		CardEffect effect = parameters.Get<CardEffect>(1, card, player) ?? CardEffect.Nothing();

		bool isComboReady = true;
		foreach (var id in idList)
		{
			Card? find = player.Card.TurnUsed.Find(card => card.Id == id);
			isComboReady = isComboReady && (find != null);
		}

		var result = new CardEffect.Result(new Result {Type = Type.Nothing});
		if (isComboReady)
		{
			result = effect.Use(preheatPhase, card, player);
		}

		return new ResultCombo {Result = result, Type = Type.Combo};	
	}
}

