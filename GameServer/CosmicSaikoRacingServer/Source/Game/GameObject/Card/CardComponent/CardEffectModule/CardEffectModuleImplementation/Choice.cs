using CSR.Game.Phase;
using ProtoBuf;

#pragma warning disable CS8618

namespace CSR.Game.GameObject.CardEffectModuleImplementation;


public class Choice : CardEffectModule
{
	[ProtoContract]
	public class ResultChoice : Result
	{
		[ProtoMember(2)]
		public CardEffect.Result Result;
	}

	public Choice(List<Parameter> parameters) : base(parameters, Type.Choice)
	{
	}

	public override Result Activate(PreheatPhase preheatPhase, Card card, GamePlayer player)
	{
		CardEffect effect = parameters.Get<CardEffect>(0, card, player) ?? CardEffect.Nothing();
		Random random = new Random();
		var result = effect.Use(random.Next(effect.Count),preheatPhase, card, player);

		return new ResultChoice() {Result = result, Type = Type.Choice};
	}
}
