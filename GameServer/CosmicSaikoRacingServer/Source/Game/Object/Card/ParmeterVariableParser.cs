
namespace CSRServer.Game
{
	internal static class ParameterVariableParser
	{
		private static readonly Dictionary<string, ResourceType> symbolToResourceType = new Dictionary<string, ResourceType>
		{
			["fossil"] = ResourceType.Fossil,
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
			["normal"] = Card.Type.Normal,
		};

		public static object Parse(GamePlayer player, string varString)
		{
			int attrIndex = varString.IndexOf('(');
			string[]? attribute = null;
			if (attrIndex > 0)
			{
				int attributeLength = varString.Length - attrIndex - 2;
				var attrString = varString.Substring(attrIndex+1, attributeLength).ToLower();
				attribute = attrString.Split(',');
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
				return GetCurrentResourceNum(player, attribute);
			}
			else if (varString == "thisTurnUsedCardNum")
			{
				return GetThisTurnUsedCardNum(player, attribute);
			}
			else if (varString == "randomCardId")
			{
				return GetRandomCardId(player, attribute);
			}
			return 0;
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
			if (attribute == null || attribute.Length < 3)
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
	}
}