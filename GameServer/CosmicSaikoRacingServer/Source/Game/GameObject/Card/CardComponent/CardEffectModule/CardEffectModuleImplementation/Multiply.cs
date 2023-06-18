using CSR.Game.Phase;
using ProtoBuf;

namespace CSR.Game.GameObject.CardEffectModuleImplementation;

public class Multiply : CardEffectModule
{
	[ProtoContract]
	public class ResultMultiply : Result
	{
		[ProtoMember(2)]
		public double Amount { get; set; }
	}

	public Multiply(List<Parameter> parameters) : base(parameters, Type.Multiply)
	{
	}

	public override Result Activate(PreheatPhase preheatPhase, Card card, GamePlayer player)
	{
		double amount = parameters.Get<double>(0, card, player);
		player.TurnDistance = (int) Math.Round(player.TurnDistance * amount);
		return new ResultMultiply {Amount = amount, Type = Type.Multiply};	
	}
}

