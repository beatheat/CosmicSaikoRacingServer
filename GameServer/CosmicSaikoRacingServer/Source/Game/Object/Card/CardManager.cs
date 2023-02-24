using System.Text.Json.Nodes;

namespace CSRServer.Game
{
	internal static class CardManager
	{
		private static readonly List<Card> cards = new List<Card>();
		private static bool _isLoaded = false;

		private static readonly Dictionary<char, ResourceType> symbolToResourceType = new Dictionary<char, ResourceType>
		{
			['f'] = ResourceType.Fossil,
			['e'] = ResourceType.Electric,
			['b'] = ResourceType.Bio,
			['n'] = ResourceType.Nuclear,
			['c'] = ResourceType.Cosmic
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
		private static readonly Dictionary<string, int> symbolToNumber = new Dictionary<string, int>
		{
			["pair"] = 2,
			["two-pair"] = 22,
			["triple"] = 3,
			["four-card"] = 4,
			["yacht"] = 5
		};
		
		public static void Load(string path)
		{
			if (_isLoaded) return;
			Logger.Log("Card effect module is loading...");
			EffectModuleManager.Load();
			Logger.LogWithClear("Card effect module is loaded");
			cards.Clear();

			Logger.Log($"Card data is loading on {path} ...");
			string jsonString = File.ReadAllText(path);
			Logger.LogWithClear("Card data is loaded");
			JsonNode jsonObject = JsonNode.Parse(jsonString)!;
			
			if (jsonObject == null)
				throw new Exception($"CardManager::Load - {path} is not json parsable");
			
			JsonArray jsonArray = jsonObject.AsArray();
			
			Logger.Log("Parsing card data...");
			for (int i = 0; i < jsonArray.Count; i++)
			{
				JsonNode jsonCard = jsonArray[i]!;
				
				var jsonId = jsonCard["id"];
				var jsonRank = jsonCard["rank"];
				var jsonType = jsonCard["type"];
				var jsonVariable = jsonCard["variable"];
				var jsonCondition = jsonCard["condition"];
				var jsonEffect = jsonCard["effect"];

				int id;
				int rank;
				Card.Type type;
				CardCondition condition;
				CardEffect effect;
				Dictionary<string, Card.Variable> variable = new Dictionary<string, Card.Variable>();

				try
				{
					if (jsonId == null || !int.TryParse(jsonId.ToString(), out id))
						throw new Exception($"CardManager::Load - index({i}) : id attribute is missing");

					if (jsonRank == null || !int.TryParse(jsonRank.ToString(), out rank))
						throw new Exception($"CardManager::Load - index({i}) : rank attribute is missing");

					if (jsonType == null || !symbolToCardType.TryGetValue(jsonType.ToString().ToLower(), out type))
						throw new Exception($"CardManager::Load - index({i}) : type attribute is missing");

					if (jsonVariable != null)
						variable = ParseCardVariable(jsonVariable.ToString());

					if (jsonCondition != null)
						condition = ParseCondition(jsonCondition.ToString());
					else
						throw new Exception($"CardManager::Load - index({i}) : condition attribute is missing");

					effect = ParseEffect(jsonEffect != null ? jsonEffect.ToString() : "Nothing()");
				}
				catch (Exception e)
				{
					throw new Exception($"CardManager::Load - index({i}) : \n" + e.Message);
				}

				cards.Add(new Card(id, type, rank, condition, effect, variable));
			}
			Logger.LogWithClear("Card data parsing ends");
			_isLoaded = true;
		}

		public static Card GetCard(int id)
		{
			if(id >= 0 && id < cards.Count)
				return cards[id].Clone();
			return cards[0].Clone();
		}
		public static Card GetRandomCardWithCondition(int rankMin, int rankMax)
		{
			Random random = new Random();
			var findCards = cards.FindAll(x => x.rank >= rankMin && x.rank <= rankMax);
			return findCards[random.Next(findCards.Count)];
		}
		public static Card GetRandomCardWithCondition(Card.Type type, int rankMin, int rankMax)
		{
			Random random = new Random();
			var findCards = cards.FindAll(x => x.type == type && x.rank >= rankMin && x.rank <= rankMax);
			return findCards[random.Next(findCards.Count)];
		}
		
		private static CardCondition ParseCondition(string rawConditionString)
		{
			bool isFreeSame = rawConditionString[0] == '#';

			if (isFreeSame == false)
			{
				List<ResourceType> conditionList = new List<ResourceType>();
				List<int> countList = new List<int>();
				
				var conditionStringSplit = rawConditionString.Split('/');
				
				foreach (var conditionString in conditionStringSplit)
				{
					char type = conditionString[0];
					if (symbolToResourceType.TryGetValue(type, out var condition))
						conditionList.Add(condition);
					else
						throw new Exception($"CardManager::ParseCondition - symbol of {rawConditionString} is not parsable");

					if (int.TryParse(conditionString.Substring(1), out int count))
						countList.Add(count);
					else
						throw new Exception($"CardManager::ParseCondition - symbol of {rawConditionString} is not parsable");
				}
				return new CardCondition(conditionList, countList);
			}
			else
			{
				var conditionString = rawConditionString.Substring(1).ToLower();
				if (symbolToNumber.TryGetValue(conditionString, out var condition))
					return new CardCondition(condition);
				else
					throw new Exception($"CardManager::ParseCondition - symbol of {rawConditionString} is not parsable");
			}
		}
		
		private static CardEffect ParseEffect(string effectString)
		{

			string[] effectModuleStringList = SplitEffectModule(effectString);
			var cardEffectElements = new List<CardEffect.Element>();
			foreach (var effectModuleString in effectModuleStringList)
			{
				// 이펙트 모듈 이름 파싱
				int delimeterIndex = effectModuleString.IndexOf('(');
				string moduleName = effectModuleString.Substring(0, delimeterIndex);

				if (!EffectModuleManager.TryGet(moduleName, out var module, out var type))
					throw new Exception($"CardManager::ParseEffect - {moduleName} is not parsable on module");
				
				// 이펙 모듈 파라미터 파싱
				string parameterListString = effectModuleString.Substring(delimeterIndex);
				var parameterStringSplit = SplitEffectParameter(parameterListString);

				// List<object> parameters = new List<object>();
				if (parameterStringSplit.Length == 1 && parameterStringSplit[0] == "")
					parameterStringSplit = Array.Empty<string>();
				var parameters = new List<CardEffect.Parameter>();
				foreach (var parameterString in parameterStringSplit)
				{
					char identifier = parameterString[0];
					if (parameterString.Contains('$') || parameterString.Contains('%'))
					{
						parameters.Add(new CardEffect.Parameter(parameterString, CardEffect.Parameter.Type.Expression));
					}
					else if (Char.IsDigit(identifier) || identifier == '-')
					{
						if(double.TryParse(parameterString,out var parameter))
							parameters.Add(new CardEffect.Parameter(parameter));
						else
							throw new Exception($"CardManager::ParseEffect - {effectString} is not parsable on number : {parameterString}");
					}
					else if (identifier is '\"' or '\'')
					{
						var parameter = parameterString.Substring(1, parameterString.Length - 2);
						parameters.Add(new CardEffect.Parameter(parameter, CardEffect.Parameter.Type.Data));
					}
					else if (identifier == '[')
					{
						var numberListString = parameterString.Substring(1, parameterString.Length - 2);
						var numberStringSplit = numberListString.Split(',', StringSplitOptions.TrimEntries);
						List<double> numberList = new List<double>();
						foreach (var numberString in numberStringSplit)
						{
							if(double.TryParse(numberString, out var number))
								numberList.Add(number);
							else
								throw new Exception($"CardManager::ParseEffect - {effectString} is not parsable on number list : {parameterString} especially at {numberString}");
						}
						parameters.Add(new CardEffect.Parameter(numberList));
					}
					else if (Char.IsLetter(identifier))
					{
						var effectInEffect = ParseEffect(parameterString);
						parameters.Add(new CardEffect.Parameter(effectInEffect));
					}
					else if (identifier == '{')
					{
						var effectListString = parameterString.Substring(1, parameterString.Length - 2);
						var effectInEffect = ParseEffect(effectListString);
						parameters.Add(new CardEffect.Parameter(effectInEffect));
					}
				}

				cardEffectElements.Add(new CardEffect.Element(module, parameters, type));
			}
			return new CardEffect(cardEffectElements);
		}

		private static Dictionary<string, Card.Variable> ParseCardVariable(string varString)
		{
			Dictionary<string, Card.Variable> variableDict = new Dictionary<string, Card.Variable>();
			var varStringSplit = varString.Split(',', StringSplitOptions.RemoveEmptyEntries);
			foreach (var param in varStringSplit)
			{
				var paramSplit = param.Split('=', StringSplitOptions.RemoveEmptyEntries);
				string variableRawName = paramSplit[0].Trim();

				//공백하나 추가
                variableRawName = variableRawName.Replace("(", "( ");
                variableRawName = variableRawName.Replace(")", " )");
                //공백 하나로 만들기
                variableRawName = Regex.Replace(variableRawName, @"\s+", " ");
                
                var variableNameAndRange = variableRawName.Split(new[] {'(', '~', ')'}, StringSplitOptions.RemoveEmptyEntries);
                string variableName;
				int lowerBound = int.MinValue;
				int upperBound = int.MaxValue;
				
				if (variableNameAndRange.Length == 1)
				{
					variableName = variableNameAndRange[0];
				} 
				else if (variableNameAndRange.Length == 3)
				{
					variableName = variableNameAndRange[0];
					if ((int.TryParse(variableNameAndRange[1], out lowerBound) && int.TryParse(variableNameAndRange[2], out upperBound)) == false)
					{
						throw new Exception($"CardManager::ParseCardVariable - Range of {variableName} is not formatted");
					}
				}
				else
				{
					throw new Exception($"CardManager::ParseCardVariable - {variableRawName} is not formatted");
				}
				
				switch (paramSplit.Length)
				{
					case 1:
						variableDict.Add(variableName, new Card.Variable {value = 0, lowerBound = lowerBound, upperBound = upperBound});
						break;
					case 2 when int.TryParse(paramSplit[1], out int initialValue):
						variableDict.Add(variableName, new Card.Variable {value = initialValue, lowerBound = lowerBound, upperBound = upperBound});
						break;
					default:
						throw new Exception($"CardManager::ParseCardVariable - {variableNameAndRange} is not formatted");
				}				
			}

			return variableDict;
		}

		private static string[] SplitEffectModule(string moduleString)
		{
			string stringSplitByAt = "";
			int cntEffectDelimeter = 0;
			//가장 바깥쪽 , 전부 @로 변환
			for (int i = 0; i < moduleString.Length; i++)
			{
				char c = moduleString[i];

				if (c == '(') cntEffectDelimeter++;
				if (c == ')') cntEffectDelimeter--;
				
				if (c == ',' && cntEffectDelimeter == 0)
					stringSplitByAt += "@";
				else
					stringSplitByAt += c;
			}

			return stringSplitByAt.Split('@', StringSplitOptions.TrimEntries);
		}
		
		private static string[] SplitEffectParameter(string parameterString)
		{
			string stringSplitByAt = "";
			bool isInNumList = false;
			int cntEffectDelimiter = 0;
			//() 때기			
			parameterString = parameterString.Substring(1, parameterString.Length - 2);
			//가장 바깥쪽 , 전부 @로 변환
			for (int i = 0; i < parameterString.Length; i++)
			{
				char c = parameterString[i];
				if (c == '[') isInNumList = true;
				if (c == ']') isInNumList = false;

				if (c == '(') cntEffectDelimiter++;
				if (c == ')') cntEffectDelimiter--;
				
				if (c == '{') cntEffectDelimiter++;
				if (c == '}') cntEffectDelimiter--;
				
				if (c == ',' && !isInNumList && cntEffectDelimiter == 0)
					stringSplitByAt += "@";
				else
					stringSplitByAt += c;
			}

			return stringSplitByAt.Split('@', StringSplitOptions.TrimEntries);
		}
	}
}