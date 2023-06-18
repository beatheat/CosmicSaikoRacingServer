using CSR.Game.Phase;
using ProtoBuf;

#pragma warning disable CS8618

namespace CSR.Game.GameObject.CardEffectModuleImplementation;

public class Draw : CardEffectModule
{
	[ProtoContract]
	public class ResultDraw : Result
	{
		[ProtoMember(2)]
		public List<Card> DrawCards { get; set; }
	}

	public Draw(List<Parameter> parameters) : base(parameters, Type.Draw)
	{
	}

	public override Result Activate(PreheatPhase preheatPhase, Card card, GamePlayer player)
	{
		int amount = parameters.Get<int>(0, card, player);

		return new ResultDraw {DrawCards = player.DrawCard(amount), Type = Type.Draw};	
	}
}
