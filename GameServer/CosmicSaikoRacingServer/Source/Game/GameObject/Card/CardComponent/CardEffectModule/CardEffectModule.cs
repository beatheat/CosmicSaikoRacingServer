namespace CSR.Game.GameObject;

public partial class CardEffectModule
{
	public readonly EffectModule effectModule;
	public readonly ParameterList parameterList;
	public readonly Type type;

	public CardEffectModule(EffectModule effectModule, List<Parameter> parameterList, Type type)
	{
		this.effectModule = effectModule;
		this.parameterList = new ParameterList(parameterList);
		this.type = type;
	}
}
