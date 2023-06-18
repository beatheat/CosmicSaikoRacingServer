using CSR.Game.Phase;
using ProtoBuf;

namespace CSR.Game.GameObject.CardEffectModuleImplementation;

public class SetVariable : CardEffectModule
{
	public SetVariable(List<Parameter> parameters) : base(parameters, Type.SetVariable)
	{
	}

	public override Result Activate(PreheatPhase preheatPhase, Card card, GamePlayer player)
	{
		string varName = parameters.Get<string>(0, card, player) ?? "";
		int number = parameters.Get<int>(1, card, player);

		if (card.variables.ContainsKey(varName))
		{
			var variable = card.variables[varName];
			variable.value += number;
			if (variable.value < variable.lowerBound)
				variable.value = variable.lowerBound;
			if (variable.value > variable.upperBound)
				variable.value = variable.upperBound;
		}

		return new Result {Type = Type.Nothing};
	}
}
