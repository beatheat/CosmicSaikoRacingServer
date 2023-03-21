namespace CSRServer.Game
{
	public class CardEffectModule
	{
		/// <summary>
		/// 카드 이펙트 모듈의 종류
		/// </summary>
		public enum Type
		{
			Nothing, Add, Multiply, Draw, RerollCountUp, Death, Fail, Initialize, ForceReroll, CreateCardToHand, 
			CreateCardToDeck, CreateCardToOther, BuffToMe, BuffToOther, EraseBuff, Overload,
			Combo, EnforceSelf, Discard, Choice, DoPercent, SetVariable, Check, Leak, Repeat, TurnEnd
		}
		
		/// <summary>
		/// 카드 이펙트 모듈의 파라미터 클래스
		/// </summary>
		public  class Parameter
		{
			public enum Type
			{
				Data,
				Expression
			}

			private readonly object _data;
			private readonly Type _type;

			public Parameter(object data, Type type = Type.Data)
			{
				this._data = data;
				this._type = type;
			}

			public T? Get<T>(Card card, GamePlayer player)
			{
				if (_type == Type.Data)
				{
					if (typeof(T) == typeof(int))
					{
						if (!double.TryParse(_data.ToString(), out var dataParsed))
							return default(T)!;
						return (T) (object) (int) dataParsed;
					}

					return (T) _data;
				}
				else if (_type == Type.Expression)
					return (T) ParameterExpressionParser.Parse(card, player, _data.ToString()!);
				else
					return default(T)!;
			}
		}

		/// <summary>
		/// 카드이펙트 모듈의 파라미터 리스트 클래스
		/// </summary>
		public class ParameterList
		{
			private readonly List<Parameter> _parameterList;

			public ParameterList(List<Parameter> parameterList)
			{
				this._parameterList = parameterList;
			}

			public T? Get<T>(int idx, Card card, GamePlayer player)
			{
				if (idx >= _parameterList.Count || idx < 0)
				{
					return default(T);
				}
				else return _parameterList[idx].Get<T>(card, player);
			}
		}

		/// <summary>
		/// 카드 이펙트모듈의 결과값 클래스
		/// </summary>
		public class Result
		{
			public object? result;
			public Type type;
		}
		
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
	
}