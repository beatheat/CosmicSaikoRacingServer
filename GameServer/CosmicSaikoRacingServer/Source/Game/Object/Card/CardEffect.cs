using System.Reflection.Metadata;

namespace CSRServer.Game
{
	using ParameterList = List<CardEffect.Parameter>;
	using EffectModule = Func<Card, GamePlayer, List<CardEffect.Parameter>, object>;
	
	internal class CardEffect
	{
		public enum Type
		{
			Nothing, Add, Multiply, Draw
		}

		public class Parameter
		{
			private readonly object data;
			private readonly bool isVariable;
			private static readonly Dictionary<string, ResourceType> symbolToResourceType = new Dictionary<string, ResourceType>
			{
				["fossil"]	= ResourceType.Fossil,
				["electric"] = ResourceType.Electric,
				["bio"] = ResourceType.Bio,
				["nuclear"] = ResourceType.Nuclear,
				["cosmic"] = ResourceType.Cosmic
			};

			private static readonly Dictionary<string, Card.Type> symbolToCardType = new Dictionary<string, Card.Type>
			{
				["fossil"] = Card.Type.Fossil,
				["electric"] = Card.Type.Electric,
				["bio"] = Card.Type.Bio,
				["nuclear"] = Card.Type.Nuclear,
				["cosmic"] = Card.Type.Cosmic,
				["normal"] = Card.Type.Normal
			};
			
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
					return (T) (object) GetVariable(player);
			}

			private int GetVariable(GamePlayer player)
			{
				string varString = data.ToString()!;
				string attribute = "";
				int attrIndex = varString.IndexOf('[');

				if (attrIndex == -1)
				{
					attribute = varString.Substring(attrIndex).ToLower();
					varString = varString.Substring(0, attrIndex);
				}

				if (varString == "currentUnusedCardNum")
				{
					return player.unusedCard.Count;
				}
				else if (varString == "currentUsedCardNum")
				{
					return player.usedCard.Count;
				}
				else if (varString == "currentHandNum")
				{
					return player.hand.Count;
				}
				else if (varString == "currentDeckNum")
				{
					return player.deck.Count;
				}
				else if (varString == "currentResourceNum")
				{
					if (attribute == "")
					{
						return player.resource.Count;
					}
					else
					{
						if (symbolToResourceType.TryGetValue(attribute, out var resourceType))
						{
							int count = 0;
							foreach (var resourceElement in player.resource)
							{
								if (resourceType == resourceElement)
									count++;
							}
							return count;
						}
					}
				}
				else if (varString == "thisTurnUsedCardNum")
				{
					if (attribute == "")
					{
						return player.turnUsedCard.Count;
					}
					else
					{
						if (symbolToCardType.TryGetValue(attribute, out var cardType))
						{
							int count = 0;
							foreach (var usedCard in player.turnUsedCard)
							{
								if (cardType == usedCard.type)
									count++;
							}
							return count;
						}
					}
				}
				return 0;
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
		
		public CardEffect(List<Element> cardEffectElements)
		{
			this.elementList = cardEffectElements;
		}

		public object[] Use(Card card, GamePlayer player)
		{
			object[] results = new object[elementList.Count];
			//리소스 체크할때 켜도 될듯하다
			card.enable = true;
			for (int i = 0; i < elementList.Count; i++)
			{
				if(card.enable)
					results[i] = elementList[i].effectModule(card, player, elementList[i].parameter);
			}
			return results;
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