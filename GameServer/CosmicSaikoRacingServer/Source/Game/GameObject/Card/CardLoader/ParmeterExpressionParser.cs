
using System.Data;
using CSR.Game;

namespace CSR.Game.GameObject;

/// <summary>
/// 이펙트 모듈의 인자 중 expression을 카드 발동 시점에 파싱하는 클래스
/// </summary>
internal static class ParameterExpressionParser
{
	// expression의 string으로 작성된 리소스 타입을 enum으로 매핑시켜주는 dictionary
	private static readonly Dictionary<string, ResourceType> _symbolToResourceType = new Dictionary<string, ResourceType>
	{
		["fossil"] = ResourceType.Fossil,
		["electric"] = ResourceType.Electric,
		["bio"] = ResourceType.Bio,
		["nuclear"] = ResourceType.Nuclear,
		["cosmic"] = ResourceType.Cosmic
	};

	// expression의 string으로 작성된 카드 타입을 enum으로 매핑시켜주는 dictionary
	private static readonly Dictionary<string, CardType> _symbolToCardType = new Dictionary<string, CardType>
	{
		["fossil"] = CardType.Fossil,
		["electric"] = CardType.Electric,
		["bio"] = CardType.Bio,
		["nuclear"] = CardType.Nuclear,
		["cosmic"] = CardType.Cosmic,
		["normal"] = CardType.Normal,
	};

	// 
	/// <summary>
	/// expression을 파싱한다
	/// </summary>
	public static object Parse(Card card, GamePlayer player, string exprString)
	{
		//expression에서 변수만 추출하여 변수부분을 실제 값으로 치환한다
		string[] variableString = GetVariablesFromExpression(exprString);
		foreach (var varString in variableString)
		{
			int variableResult;
			//전역 변수일 경우 
			if (varString[0] == '%')
				variableResult = GetGlobalVariable(player, varString[1..]);
			//카드 변수일 경우
			else //(varString[0] == '$')
				variableResult = GetCardVariable(card, varString[1..]);

			exprString = exprString.Replace(varString, variableResult.ToString());
		}

		//실제 값으로 치환된 문자열을 단일 값으로 계산한다
		return Calculate(exprString);
	}

	/// <summary>
	/// 내장된 전역 변수를 파싱한다
	/// </summary>
	private static int GetGlobalVariable(GamePlayer player, string varString)
	{
		string variable = varString;

		//[]안에 들어있는 변수 attribute를 찾는다
		int attrIndex = variable.IndexOf('[');
		string[]? attribute = null;

		if (attrIndex > 0)
		{
			int attributeLength = variable.Length - attrIndex - 2;
			var attrString = variable.Substring(attrIndex + 1, attributeLength).ToLower();
			attribute = attrString.Split(',');
			variable = variable.Substring(0, attrIndex);
		}

		//각 변수를 attribute와 함께 파싱한다
		int result = variable switch
		{
			"currentUnusedCardNum" => player.Card.Unused.Count,
			"currentUsedCardNum" => player.Card.Used.Count,
			"currentHandNum" => player.Card.Hand.Count,
			"currentDeckNum" => player.Card.Deck.Count,
			"currentResourceReelNum" => player.Resource.ReelCount,
			"currentResourceNum" => GetCurrentResourceNum(player, attribute),
			"thisTurnUsedCardNum" => GetThisTurnUsedCardNum(player, attribute),
			"randomCardId" => GetRandomCardId(attribute),
			"randomNum" => GetRandomNum(attribute),
			"currentMyBuffNum" => GetCurrentMyBuffNum(player, attribute),
			_ => 0
		};

		return result;
	}

	/// <summary>
	/// 내장된 카드 변수를 파싱한다
	/// </summary>
	public static int GetCardVariable(Card card, string varString)
	{
		if (card.variables.TryGetValue(varString, out var result))
		{
			return result.value;
		}

		return 0;
	}

	/// <summary>
	/// expression에서 상수를 제외한 변수부분만 뽑는다
	/// </summary>
	private static string[] GetVariablesFromExpression(string exprString)
	{
		List<string> variableList = new List<string>();
		string varString = "";
		bool readVariable = false;
		//char를 하나씩 읽으면서 변수만 추출
		for (int idx = 0; idx < exprString.Length; idx++)
		{
			//%나 $로 시작하면 변수
			if (exprString[idx] is '%' or '$')
			{
				readVariable = true;
			}

			//카드변수의 종결자를 만나면 이 시점까지 읽은 문자열을 변수로 판단한다
			if (readVariable && exprString[idx] is '+' or '-' or '*' or '/' or '=' or '>' or '<' or '&' or '|' or ')')
			{
				variableList.Add(varString.TrimEnd());
				varString = "";
				readVariable = false;
			}

			//읽기 상태
			if (readVariable)
				varString += exprString[idx];
		}

		//마지막 원소로 변수가 있다면 추가
		if (varString.Length > 0)
			variableList.Add(varString.TrimEnd());
		return variableList.ToArray();
	}

	/// <summary>
	/// string으로 된 사칙연산식을 계산한다
	/// </summary>
	private static object Calculate(string varString)
	{
		object calcValue = new DataTable().Compute(varString, null);
		double.TryParse(calcValue.ToString(), out var value);
		return value;
	}

	/// <summary>
	/// 전역 변수 계산: 현재 플레이어의 리소스에 존재하는 리소스 개수 
	/// </summary>
	private static int GetCurrentResourceNum(GamePlayer player, string[]? attribute)
	{
		if (attribute == null)
			return 0;
		//all일 경우 모든 리소스릴 개수
		if (attribute[0] == "all")
			return player.Resource.Reel.Count;

		// resource.type에 맞는 리소스 개수
		if (_symbolToResourceType.TryGetValue(attribute[0], out var resourceType))
		{
			int count = 0;
			foreach (var resourceElement in player.Resource.Reel)
			{
				if (resourceType == resourceElement)
					count++;
			}

			return count;
		}

		return 0;
	}

	/// <summary>
	/// 전역 변수 계산: 이번턴에 사용된 카드 개수 
	/// </summary>
	private static int GetThisTurnUsedCardNum(GamePlayer player, string[]? attribute)
	{
		if (attribute == null)
			return 0;
		//all일 경우 사용된 모든 카드 개수
		if (attribute[0] == "all")
			return player.Card.TurnUsed.Count;
		//card.type에 맞는 카드 개수
		if (_symbolToCardType.TryGetValue(attribute[0], out var cardType))
		{
			int count = 0;
			foreach (var usedCard in player.Card.TurnUsed)
			{
				if (cardType == usedCard.Type)
					count++;
			}

			return count;
		}

		return 0;
	}

	/// <summary>
	/// 전역 변수 계산 : 랜덤한 카드 ID
	/// </summary>
	private static int GetRandomCardId(string[]? attribute)
	{
		if (attribute == null)
			return 0;
		if (attribute.Length < 3)
			return 0;
		//rankMin과 rankMax 사이의 랜덤한 카드 ID
		if (int.TryParse(attribute[1], out int rankMin) && int.TryParse(attribute[2], out int rankMax))
		{
			//all일 경우 모든 타입 중 랜덤한 카드 ID
			if (attribute[0] == "all")
			{
				return CardManager.GetRandomCardWithCondition(rankMin, rankMax).Id;
			}
			//card.type의 카드 중 랜덤한 카드 ID
			else if (_symbolToCardType.TryGetValue(attribute[0], out var cardType))
			{
				return CardManager.GetRandomCardWithCondition(cardType, rankMin, rankMax).Id;
			}
		}

		return 0;
	}

	/// <summary>
	/// 전역 변수 계산 : 랜덤한 수
	/// </summary>
	private static int GetRandomNum(string[]? attribute)
	{
		if (attribute == null)
			return 0;
		if (attribute.Length < 2)
			return 0;
		// min<=x<=max 인 랜덤한 x
		if (int.TryParse(attribute[0], out int min) && int.TryParse(attribute[1], out int max))
		{
			return new Random().Next(min, max + 1);
		}

		return 0;
	}

	/// <summary>
	/// 전역 변수 계산 : 현재 버프의 수 
	/// </summary>
	private static int GetCurrentMyBuffNum(GamePlayer player, string[]? attribute)
	{
		if (attribute == null)
		{
			return player.Buff.List.Sum(buff => buff.Count);
		}

		if (attribute.Length >= 2)
			return 0;
		//버프 id의 버프스택이 몇개나 있는지 반환
		if (int.TryParse(attribute[0], out int buffId))
		{
			return player.GetBuffCount((BuffType) buffId);
		}

		return 0;
	}
}