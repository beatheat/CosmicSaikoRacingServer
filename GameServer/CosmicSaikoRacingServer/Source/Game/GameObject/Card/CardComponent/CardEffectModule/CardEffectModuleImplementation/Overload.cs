using CSR.Game.Phase;
using ProtoBuf;

#pragma warning disable CS8618

namespace CSR.Game.GameObject.CardEffectModuleImplementation;

public class Overload : CardEffectModule
{
	[ProtoContract]
	public class ResultOverload : Result
	{
		[ProtoMember(2)]
		public CardEffect.Result Result { get; set; }
	}

	public Overload(List<Parameter> parameters) : base(parameters, Type.Overload)
	{
	}

	public override Result Activate(PreheatPhase preheatPhase, Card card, GamePlayer player)
	{
		string percent = parameters.Get<string>(0, card, player) ?? "";
		int amount = parameters.Get<int>(1, card, player);
		CardEffect effect = parameters.Get<CardEffect>(2, card, player) ?? CardEffect.Nothing();
		;

		var result = new CardEffect.Result(new Result {Type = Type.Nothing});

		if (card.variables.ContainsKey(percent))
		{
			var overloadPercent = card.variables[percent];
			Random random = new Random();
			if (random.Next(100) < overloadPercent.value)
			{
				result = effect.Use(preheatPhase, card, player);
			}

			overloadPercent.value += amount;
			if (overloadPercent.value < overloadPercent.lowerBound)
				overloadPercent.value = overloadPercent.lowerBound;
			if (overloadPercent.value > overloadPercent.upperBound)
				overloadPercent.value = overloadPercent.upperBound;
		}

		return new ResultOverload {Result= result, Type = Type.Overload};	
	}
}

