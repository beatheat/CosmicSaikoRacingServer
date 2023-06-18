using System.Reflection;
using CSR.Game.Phase;

namespace CSR.Game.GameObject;

public abstract partial class CardEffectModule
{
	public readonly ParameterList parameters;
	public readonly Type type;

	protected CardEffectModule(List<Parameter> parameters, Type type)
	{
		this.parameters = new ParameterList(parameters);
		this.type = type;
	}

	public abstract Result Activate(PreheatPhase preheatPhase, Card card, GamePlayer player);
	
	public static CardEffectModule Create(string moduleName, List<Parameter> parameters)
	{
		var baseType = typeof(CardEffectModule);
		var currentAssembly = Assembly.GetExecutingAssembly();
		var derivedTypes = currentAssembly.GetTypes().Where(type => type != baseType && baseType.IsAssignableFrom(type)).ToList();

		CardEffectModule? cardEffectModule = null;
		foreach (var derivedType in derivedTypes)
		{
			if (derivedType.Name == moduleName)
			{
				cardEffectModule = (CardEffectModule) Activator.CreateInstance(derivedType, parameters)!;
			}
		}

		if (cardEffectModule == null)
			throw new Exception("There is no Effect Module with such a name : "  + moduleName);
		return cardEffectModule;
	}
}
