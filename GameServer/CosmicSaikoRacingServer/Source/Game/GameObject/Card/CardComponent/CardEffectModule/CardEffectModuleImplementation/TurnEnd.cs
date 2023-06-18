using CSR.Game.Phase;
using ProtoBuf;

namespace CSR.Game.GameObject.CardEffectModuleImplementation;

public class TurnEnd : CardEffectModule
{
	public TurnEnd(List<Parameter> parameters) : base(parameters, Type.TurnEnd)
	{
	}

	public override Result Activate(PreheatPhase preheatPhase, Card card, GamePlayer player)
	{
		card.Enable = false;
		preheatPhase.Ready(player);
		return new Result {Type = Type.TurnEnd};
	}
}
