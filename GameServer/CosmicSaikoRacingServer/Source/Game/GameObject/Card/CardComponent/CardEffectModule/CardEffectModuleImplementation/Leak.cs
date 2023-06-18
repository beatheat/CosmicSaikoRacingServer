using CSR.Game.Phase;
using ProtoBuf;

#pragma warning disable CS8618

namespace CSR.Game.GameObject.CardEffectModuleImplementation;

public class Leak : CardEffectModule
{
	[ProtoContract]
	public class ResultLeak : Result
	{
		[ProtoMember(2)]
		public CardEffect.Result Result { get; set; }
	}

	public Leak(List<Parameter> parameters) : base(parameters, Type.Leak)
	{
	}

	public override Result Activate(PreheatPhase preheatPhase, Card card, GamePlayer player)
	{
		CardEffect effect = parameters.Get<CardEffect>(0, card, player) ?? CardEffect.Nothing();
		var result = effect.Use(preheatPhase, card, player);
		return new ResultLeak {Result = result, Type = Type.Leak};	
	}
}