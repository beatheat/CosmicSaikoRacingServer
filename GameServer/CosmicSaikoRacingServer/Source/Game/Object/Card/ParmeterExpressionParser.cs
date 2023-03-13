
using System.Data;

namespace CSRServer.Game
{
	internal static class ParameterExpressionParser
	{
		private static readonly Dictionary<string, Resource.Type> symbolToResourceType = new Dictionary<string, Resource.Type>
		{
			["fossil"] = Resource.Type.Fossil,
			["electric"] = Resource.Type.Electric,
			["bio"] = Resource.Type.Bio,
			["nuclear"] = Resource.Type.Nuclear,
			["cosmic"] = Resource.Type.Cosmic
		};

		private static readonly Dictionary<string, Card.Type> symbolToCardType = new Dictionary<string, Card.Type>
		{
			["fossil"] = Card.Type.Fossil,
			["electric"] = Card.Type.Electric,
			["bio"] = Card.Type.Bio,
			["nuclear"] = Card.Type.Nuclear,
			["cosmic"] = Card.Type.Cosmic,
			["normal"] = Card.Type.Normal,
		};

		public static object Parse(Card card, GamePlayer player, string exprString)
		{
			string[] variableString = GetVariablesFromExpression(exprString);
			foreach (var varString in variableString)
			{
				int variableResult;
				if (varString[0] == '%')					
					variableResult = GetGlobalVariable(player, varString[1..]);
				else //(varString[0] == '$')
					variableResult = GetCardVariable(card, varString[1..]);
				
				exprString = exprString.Replace(varString, variableResult.ToString());
			}

			return Calculate(exprString);
		}
		
		public static int GetGlobalVariable(GamePlayer player, string varString)
		{
			string variable = varString;
			
			int attrIndex = variable.IndexOf('[');
			string[]? attribute = null;
			
			
			if (attrIndex > 0)
			{
				int attributeLength = variable.Length - attrIndex - 2;
				var attrString = variable.Substring(attrIndex+1, attributeLength).ToLower();
				attribute = attrString.Split(',');
				variable = variable.Substring(0, attrIndex);
			}
			
			int result = variable switch
			{
				"currentUnusedCardNum" => player.unusedCard.Count,
				"currentUsedCardNum" => player.usedCard.Count,
				"currentHandNum" => player.hand.Count,
				"currentDeckNum" => player.deck.Count,
				"currentResourceReelNum" => player.resourceReelCount,
				"currentResourceNum" => GetCurrentResourceNum(player, attribute),
				"thisTurnUsedCardNum" => GetThisTurnUsedCardNum(player, attribute),
				"randomCardId" => GetRandomCardId(player, attribute),
				"randomNum" => GetRandomNum(player, attribute),
				"currentMyBuffNum" => GetCurrentMyBuffNum(player, attribute),
				_ => 0
			};

			return result;
		}
		
		public static int GetCardVariable(Card card, string varString)
		{
			if (card.variable.TryGetValue(varString, out var result))
			{
				return result.value;
			}
			return 0;
		}

		private static string[] GetVariablesFromExpression(string exprString)
		{
			List<string> variableList = new List<string>();
			string varString = "";
			bool readVariable = false;
			for (int idx = 0; idx < exprString.Length; idx++)
			{
				if (exprString[idx] is '%' or '$')
				{
					readVariable = true;
				}
				if (readVariable && exprString[idx] is '+' or '-' or '*' or '/' or '=' or '>' or '<' or '&' or '|' or ')')
				{
					variableList.Add(varString.TrimEnd());
					varString = "";
					readVariable = false;
				}
				if(readVariable)
					varString += exprString[idx];
			}
			if(varString.Length > 0)
				variableList.Add(varString.TrimEnd());
			return variableList.ToArray();
		}
		
		// 정수 계산기
		private static object Calculate(string varString)
		{
			return new DataTable().Compute(varString, null);
		}

		private static int GetCurrentResourceNum(GamePlayer player, string[]? attribute)
		{
			if (attribute == null)
				return 0;
			if (attribute[0] == "all")
				return player.resourceReel.Count;
			
			if (symbolToResourceType.TryGetValue(attribute[0], out var resourceType))
			{
				int count = 0;
				foreach (var resourceElement in player.resourceReel)
				{
					if (resourceType == resourceElement)
						count++;
				}
				return count;
			}
			return 0;
		}

		private static int GetThisTurnUsedCardNum(GamePlayer player, string[]? attribute)
		{
			if (attribute == null)
				return 0;
			if (attribute[0] == "all")
				return player.turnUsedCard.Count;
			if (symbolToCardType.TryGetValue(attribute[0], out var cardType))
			{
				int count = 0;
				foreach (var usedCard in player.turnUsedCard)
				{
					if (cardType == usedCard.type)
						count++;
				}

				return count;
			}

			return 0;
		}

		private static int GetRandomCardId(GamePlayer player, string[]? attribute)
		{
			if (attribute == null) 
				return 0;
			if (attribute.Length < 3)
				return 0;
			if (int.TryParse(attribute[1], out int rankMin) && int.TryParse(attribute[2], out int rankMax))
			{
				if (attribute[0] == "all")
				{
					return CardManager.GetRandomCardWithCondition(rankMin, rankMax).id;
				}
				else if (symbolToCardType.TryGetValue(attribute[0], out var cardType))
				{
					return CardManager.GetRandomCardWithCondition(cardType, rankMin, rankMax).id;
				}
			}
			return 0;
		}

		private static int GetRandomNum(GamePlayer player, string[]? attribute)
		{
			if (attribute == null)
				return 0;
			if (attribute.Length < 2)
				return 0;
			if (int.TryParse(attribute[0], out int min) && int.TryParse(attribute[1], out int max))
			{
				return new Random().Next(min, max);
			}
			return 0;
		}

		private static int GetCurrentMyBuffNum(GamePlayer player, string[]? attribute)
		{
			if (attribute == null)
			{
				return player.buffManager.buffList.Sum(buff => buff.count);
			}
			if (attribute.Length >= 2)
				return 0;
			if (int.TryParse(attribute[0], out int buffId))
			{
				return player.buffManager.GetBuffCount((Buff.Type) buffId);
			}
			return 0;
		}
	}
}