using CSR.Game.Phase;
using ProtoBuf;

namespace CSR.Game.GameObject.CardEffectModuleImplementation;


public class BuffToMe : CardEffectModule
{
	[ProtoContract]
	public class ResultBuffToMe : Result
	{
		[ProtoMember(2)]
		public BuffType BuffType { get; set; }
		[ProtoMember(3)]
		public int BuffCount { get; set; }
	}
	
	public BuffToMe(List<Parameter> parameters) : base(parameters, Type.BuffToMe)
	{
	}

	public override Result Activate(PreheatPhase preheatPhase, Card card, GamePlayer player)
	{
		int id = parameters.Get<int>(0, card, player);
		int amount = parameters.Get<int>(1, card, player);

		player.AddBuff((BuffType) id, amount);

		return new ResultBuffToMe {BuffType = (BuffType)id, BuffCount = amount, Type = Type.BuffToMe};	
	}
}
