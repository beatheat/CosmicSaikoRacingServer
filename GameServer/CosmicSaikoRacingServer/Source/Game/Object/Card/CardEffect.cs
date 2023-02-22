using System.Reflection.Metadata;

namespace CSRServer.Game
{
	using ParameterList = List<CardEffect.Parameter>;

	internal class CardEffect
	{
		public enum Type
		{
			Nothing, Add, Multiply, Draw, RerollCountUp, Death, Fail, Initialize, ForceReroll, CreateCardToHand, 
			CreateCardToDeck, CreateCardToOther, BuffToMe, BuffToOther, EraseBuff, Overload, MountCard, MountBuff,
			Combo, EnforceSelf, Discard, Choice, DoPercent, SetVariable, 
		}

		public struct Result
		{
			public object? result;
			public Type type;
		}
		
		public class Parameter
		{
			public enum Type
			{
				Data, Expression
			}
			private readonly object data;
			private readonly Type type;

			public Parameter(object data, Type type = Type.Data)
			{
				this.data = data;
				this.type = type;
			}

			public T Get<T>(Card card, GamePlayer player)
			{
				if (type == Type.Data)
				{
					if (typeof(T) == typeof(int) || typeof(T) == typeof(double))
					{
						if (!double.TryParse(data.ToString(), out var d))
							return default(T)!;
					}
					if (typeof(T) == typeof(int))
					{
						return (T) (object) (int) (double) data;
					}
					return (T) data;
				}
				else if(type == Type.Expression)
					return (T) ParameterExpressionParser.Parse(card, player, data.ToString()!);
				else
					return default(T)!;
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