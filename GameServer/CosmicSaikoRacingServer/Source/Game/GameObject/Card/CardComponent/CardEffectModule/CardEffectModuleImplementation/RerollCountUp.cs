using CSR.Game.Phase;
using ProtoBuf;

namespace CSR.Game.GameObject.CardEffectModuleImplementation;

public class RerollCountUp : CardEffectModule
{
	[ProtoContract]
	public class ResultRerollCountUp : Result
	{
		[ProtoMember(2)]
		public int Amount { get; set; }
	}

	public RerollCountUp(List<Parameter> parameters) : base(parameters, Type.RerollCountUp)
	{
	}

	public override Result Activate(PreheatPhase preheatPhase, Card card, GamePlayer player)
	{
		int amount = parameters.Get<int>(0, card, player);
		player.AddRerollCount(amount);
		
		return new ResultRerollCountUp {Amount = amount, Type = Type.RerollCountUp};
		
	}
}
