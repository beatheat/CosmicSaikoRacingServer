using CSR.Game.Phase;
using ProtoBuf;

namespace CSR.Game.GameObject.CardEffectModuleImplementation;


public class Fail : CardEffectModule
{
	public Fail(List<Parameter> parameters) : base(parameters, Type.Fail)
	{
	}

	public override Result Activate(PreheatPhase preheatPhase, Card card, GamePlayer player)
	{
		card.Enable = false;
		return new Result { Type = Type.Fail};
	}
}