using CSR.Game.Phase;
using ProtoBuf;

#pragma warning disable CS8618

namespace CSR.Game.GameObject.CardEffectModuleImplementation;

public class Repeat : CardEffectModule
{
	[ProtoContract]
	public class ResultRepeat : Result
	{
		[ProtoMember(2)]
		public List<CardEffect.Result> Results { get; set; }
	}

	public Repeat(List<Parameter> parameters) : base(parameters, Type.Repeat)
	{
	}

	public override Result Activate(PreheatPhase preheatPhase, Card card, GamePlayer player)
	{
		int amount = parameters.Get<int>(0, card, player);
		CardEffect effect = parameters.Get<CardEffect>(1, card, player) ?? CardEffect.Nothing();

		var results = new List<CardEffect.Result>();
		for (int i = 0; i < amount; i++)
			results.Add(effect.Use(preheatPhase, card, player));
		return new ResultRepeat {Results = results, Type = Type.Repeat};	
	}
}
