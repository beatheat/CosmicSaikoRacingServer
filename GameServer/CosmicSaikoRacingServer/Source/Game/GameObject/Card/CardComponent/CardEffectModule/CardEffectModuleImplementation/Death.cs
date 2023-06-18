using CSR.Game.Phase;
using ProtoBuf;

namespace CSR.Game.GameObject.CardEffectModuleImplementation;


public class Death : CardEffectModule
{
	public Death(List<Parameter> parameters) : base(parameters, Type.Death)
	{
	}

	public override Result Activate(PreheatPhase preheatPhase, Card card, GamePlayer player)
	{
		card.Death = true;
		return new Result {Type = Type.Death};
	}
}