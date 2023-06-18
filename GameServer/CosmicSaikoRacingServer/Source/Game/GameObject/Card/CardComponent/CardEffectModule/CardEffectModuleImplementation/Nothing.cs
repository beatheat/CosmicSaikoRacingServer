using CSR.Game.Phase;
using ProtoBuf;

namespace CSR.Game.GameObject.CardEffectModuleImplementation;


public class Nothing : CardEffectModule
{
	public Nothing(List<Parameter> parameters) : base(parameters, Type.Nothing) { }

	public override Result Activate(PreheatPhase preheatPhase, Card card, GamePlayer player)
	{
		return new Result { Type = Type.Nothing};
	}
}