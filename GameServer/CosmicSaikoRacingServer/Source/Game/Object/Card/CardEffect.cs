using System.Reflection.Metadata;

namespace CSRServer.Game
{
	using ParameterList = List<CardEffect.Parameter>;

	internal class CardEffect
	{
		public enum Type
		{
			Nothing, Add, Multiply, Draw, RerollCountUp, Death, Fail, 
			Initialize, ForceReroll, CreateToMe, CreateToOther, BuffToMe, BuffToOther, EraseBuff, Overload, Mount,
			Combo, EnforceSelf, Discard, Choice
		}

		public struct Result
		{
			public object? result;
			public Type type;
		}
		
		public class Parameter
		{
			private readonly object data;
			private readonly bool isVariable;

			public Parameter(object data, bool isVariable = false)
			{
				this.data = data;
				this.isVariable = isVariable;
			}

			public T Get<T>(GamePlayer player)
			{
				if (isVariable == false)
				{
					if (typeof(T) == typeof(int))
					{
						return (T) (object) (int) (double) data;
					}
					return (T) data;
				}
				else
					return (T) ParameterVariableParser.Parse(player, data.ToString()!);
			}

		}
		
		public struct Element
		{
			public EffectModule effectModule;
			public ParameterList parameter;
			public Type type;

			public Element(EffectModule effectModule, ParameterList parameter, Type type)
			{
				this.effectModule = effectModule;
				this.parameter = parameter;
				this.type = type;
			}
		}

		private readonly List<Element> elementList;
		public int ElementCount => elementList.Count;

		public CardEffect(List<Element> cardEffectElements)
		{
			this.elementList = cardEffectElements;
		}

		public Result[] Use(Card card, GamePlayer player)
		{
			Result[] results = new Result[elementList.Count];
			for (int i = 0; i < elementList.Count; i++)
			{
				if(card.enable)
					results[i] = elementList[i].effectModule(card, player, elementList[i].parameter);
			}
			return results;
		}
		
		public Result Use(int index, Card card, GamePlayer player)
		{
			if (index < 0 || index > ElementCount)
				index = 0;
			return elementList[index].effectModule(card, player, elementList[index].parameter);;
		}

		public Type[] GetTypes()
		{
			Type[] typeList = new Type[elementList.Count];
			for(int i=0;i<elementList.Count;i++)
			{
				typeList[i] = elementList[i].type;
			}
			return typeList;
		}
	}
	
}