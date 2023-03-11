using System.Reflection.Metadata;

namespace CSRServer.Game
{
	// using ParameterList = List<CardEffect.Parameter>;

	public class CardEffect
	{
		public enum Type
		{
			Nothing, Add, Multiply, Draw, RerollCountUp, Death, Fail, Initialize, ForceReroll, CreateCardToHand, 
			CreateCardToDeck, CreateCardToOther, BuffToMe, BuffToOther, EraseBuff, Overload, MountCard, MountBuff,
			Combo, EnforceSelf, Discard, Choice, DoPercent, SetVariable, Check, Leak, Repeat, TurnEnd
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

			public T? Get<T>(Card card, GamePlayer player)
			{
				if (type == Type.Data)
				{
					if (typeof(T) == typeof(int))
					{
						if (!double.TryParse(data.ToString(), out var dataParsed))
							return default(T)!;
						return (T) (object) (int) dataParsed;
					}
					return (T) data;
				}
				else if(type == Type.Expression)
					return (T) ParameterExpressionParser.Parse(card, player, data.ToString()!);
				else
					return default(T)!;
			}
		}

		public class ParameterList
		{
			private readonly List<Parameter> parameterList;

			public ParameterList(List<Parameter> parameterList)
			{
				this.parameterList = parameterList;
			}

			public T? Get<T>(int idx, Card card, GamePlayer player)
			{
				if (idx >= parameterList.Count || idx < 0)
				{
					return default(T);
				}
				else return parameterList[idx].Get<T>(card, player);
			}
		}
		
		public struct Element
		{
			public EffectModule effectModule;
			public ParameterList parameter;
			public Type type;

			public Element(EffectModule effectModule, List<Parameter> parameter, Type type)
			{
				this.effectModule = effectModule;
				this.type = type;
				this.parameter = new ParameterList(parameter);
			}
		}

		private readonly List<Element> elementList;
		public int ElementCount => elementList.Count;

		public CardEffect(List<Element> cardEffectElements)
		{
			this.elementList = cardEffectElements;
		}

		public static CardEffect Nothing()
		{
			EffectModuleManager.TryGet("Nothing", out var nothingEffect, out var type);
			return new CardEffect(new List<Element>{new Element(nothingEffect, null!, type)});
		}

		public Result[] Use(Card card, GamePlayer player)
		{
			Result[] results = new Result[elementList.Count];
			for (int i = 0; i < elementList.Count; i++)
			{
				// 버리기 효과는 무시
				if (elementList[i].type == Type.Leak)
					continue;
				
				if(card.enable)
					results[i] = elementList[i].effectModule(card, player, elementList[i].parameter);
			}
			return results;
		}

		public Result[] UseLeak(Card card, GamePlayer player)
		{
			Result[] results = new Result[elementList.Count];
			for (int i = 0; i < elementList.Count; i++)
			{
				// 버리기 효과가 아닐시 무시
				if (elementList[i].type != Type.Leak)
					continue;
				
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