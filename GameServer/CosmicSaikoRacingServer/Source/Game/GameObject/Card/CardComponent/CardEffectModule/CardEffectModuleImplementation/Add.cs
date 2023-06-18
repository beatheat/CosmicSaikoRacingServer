using CSR.Game.Phase;
using ProtoBuf;

namespace CSR.Game.GameObject.CardEffectModuleImplementation;


public class Add : CardEffectModule
{
	[ProtoContract]
	public class ResultAdd : Result
	{
		[ProtoMember(2)]
		public int Amount { get; set; }
	}

	public Add(List<Parameter> parameters) : base(parameters, Type.Add) { }

	public override Result Activate(PreheatPhase preheatPhase, Card card, GamePlayer player)
	{
		int amount = parameters.Get<int>(0, card, player);
		player.TurnDistance += amount;
		return new ResultAdd { Amount = amount, Type = Type.Add};
	}
}
