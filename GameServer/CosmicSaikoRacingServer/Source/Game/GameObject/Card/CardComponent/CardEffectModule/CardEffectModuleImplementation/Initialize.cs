using CSR.Game.Phase;
using ProtoBuf;

namespace CSR.Game.GameObject.CardEffectModuleImplementation;


public class Initialize : CardEffectModule
{
	public Initialize(List<Parameter> parameters) : base(parameters, Type.Initialize)
	{
	}

	public override Result Activate(PreheatPhase preheatPhase, Card card, GamePlayer player)
	{
		player.TurnDistance = 0;
		return new Result { Type = Type.Initialize};
	}
}