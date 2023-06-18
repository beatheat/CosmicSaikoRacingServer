using CSR.Game.Phase;
using ProtoBuf;

#pragma warning disable CS8618

namespace CSR.Game.GameObject.CardEffectModuleImplementation;

public class Check : CardEffectModule
{
	[ProtoContract]
	public class ResultCheck : Result
	{
		[ProtoMember(2)]
		public CardEffect.Result Result { get; set; }
	}

	public Check(List<Parameter> parameters) : base(parameters, Type.Check)
	{
	}

	public override Result Activate(PreheatPhase preheatPhase, Card card, GamePlayer player)
	{
		bool condition = parameters.Get<bool>(0, card, player);
		CardEffect effect = parameters.Get<CardEffect>(1, card, player) ?? CardEffect.Nothing();

		var result = new CardEffect.Result(new CardEffectModule.Result {Type = Type.Nothing});
		if (condition)
			result = effect.Use(preheatPhase, card, player);

		return new ResultCheck {Result = result, Type = Type.Check};
		
	}
}
