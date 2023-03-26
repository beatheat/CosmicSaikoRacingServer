using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace CSRServer.Game
{
	/// <summary>
	/// 카드 데이터를 로딩하는 클래스
	/// </summary>
	internal static class CardManager
	{
		private static readonly List<Card> _cards = new List<Card>();
		private static bool _isLoaded = false;

		// 카드 조건의 문자를 resource.type으로 매핑
		private static readonly Dictionary<char, Resource.Type> _symbolToResourceType = new Dictionary<char, Resource.Type>
		{
			['f'] = Resource.Type.Fossil,
			['e'] = Resource.Type.Electric,
			['b'] = Resource.Type.Bio,
			['n'] = Resource.Type.Nuclear,
			['c'] = Resource.Type.Cosmic
		};
		// 카드 조건의 common type을 수로 변경
		private static readonly Dictionary<string, int> _symbolToNumber = new Dictionary<string, int>
		{
			["pair"] = 2,
			["two-pair"] = 22,
			["triple"] = 3,
			["four-card"] = 4,
			["yacht"] = 5
		};
		// string으로 작성된 card.type으로 매핑시켜주는 dictionary
		private static readonly Dictionary<string, Card.Type> _symbolToCardType = new Dictionary<string, Card.Type>
		{
			["fossil"] = Card.Type.Fossil,
			["electric"] = Card.Type.Electric,
			["bio"] = Card.Type.Bio,
			["nuclear"] = Card.Type.Nuclear,
			["cosmic"] = Card.Type.Cosmic,
			["normal"] = Card.Type.Normal
		};

		/// <summary>
		/// 카드 데이터를 로딩한다
		/// </summary>
		public static void Load(string path)
		{
			if (_isLoaded) return;
			Logger.Log("Card effect module is loading...");
			//이펙트 모듈을 로드
			EffectModuleManager.Load();
			Logger.LogWithClear("Card effect module is loaded");
			_cards.Clear();

			Logger.Log($"Card data is loading on {path} ...");
			//카드데이터의 json string을 로드
			string jsonString = File.ReadAllText(path);
			Logger.LogWithClear("Card data is loaded");
			JsonNode jsonObject = JsonNode.Parse(jsonString)!;
			
			if (jsonObject == null)
				throw new Exception($"CardManager::Load - {path} is not json parsable");
			
			JsonArray jsonArray = jsonObject.AsArray();
			
			Logger.Log("Parsing card data...");
			//카드데이터 파싱
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
				bool death;
				try
				{
					if (jsonId == null || !int.TryParse(jsonId.ToString(), out id))
						throw new Exception($"CardManager::Load - index({i}) : id attribute is missing");

					if (jsonRank == null || !int.TryParse(jsonRank.ToString(), out rank))
						throw new Exception($"CardManager::Load - index({i}) : rank attribute is missing");

					if (jsonType == null || !_symbolToCardType.TryGetValue(jsonType.ToString().ToLower(), out type))
						throw new Exception($"CardManager::Load - index({i}) : type attribute is missing");

					if (jsonVariable != null)
						variable = ParseCardVariable(jsonVariable.ToString());

					if (jsonCondition != null)
						condition = ParseCondition(jsonCondition.ToString());
					else
						throw new Exception($"CardManager::Load - index({i}) : condition attribute is missing");

					effect = ParseEffect(jsonEffect != null ? jsonEffect.ToString() : "Nothing()", out death);
				}
				catch (Exception e)
				{
					throw new Exception($"CardManager::Load - index({i}) : \n" + e.Message);
				}

				_cards.Add(new Card(id, type, rank, condition, effect, variable, death));
			}
			Logger.LogWithClear("Card data parsing ends");
			_isLoaded = true;
		}

		/// <summary>
		/// id를 통해 원본 카드의 복제본을 얻는다
		/// </summary>
		public static Card GetCard(int id)
		{
			if(id >= 0 && id < _cards.Count)
				return _cards[id].Clone();
			return _cards[0].Clone();
		}
		/// <summary>
		/// rankMin과 rankMax 사이의 랜덤한 카드의 복제본을 얻는다
		/// </summary>
		public static Card GetRandomCardWithCondition(int rankMin, int rankMax)
		{
			Random random = new Random();
			var findCards = _cards.FindAll(x => x.rank >= rankMin && x.rank <= rankMax);
			return findCards[random.Next(findCards.Count)].Clone();
		}
		/// <summary>
		/// card.type의 카드 중 rankMin과 rankMax 사이의 랜덤한 카드의 복제본을 얻는다
		/// </summary>
		public static Card GetRandomCardWithCondition(Card.Type type, int rankMin, int rankMax)
		{
			Random random = new Random();
			var findCards = _cards.FindAll(x => x.type == type && x.rank >= rankMin && x.rank <= rankMax);
			return findCards[random.Next(findCards.Count)].Clone();
		}
		
		/// <summary>
		/// 카드 조건을 파싱한다
		/// </summary>
		private static CardCondition ParseCondition(string rawConditionString)
		{
			bool isCommonType = rawConditionString[0] == '#';

			if (isCommonType == false)
			{
				List<Resource.Type> conditionList = new List<Resource.Type>();
				
				var conditionStringSplit = rawConditionString.Split('/');
				
				foreach (var conditionString in conditionStringSplit)
				{
					char type = conditionString.Trim()[0];
					if (_symbolToResourceType.TryGetValue(type, out var condition) && int.TryParse(conditionString[1..], out int count))
					{
						for(int i=0;i<count;i++)
							conditionList.Add(condition);
					}
					else
						throw new Exception($"CardManager::ParseCondition - symbol of {rawConditionString} is not parsable");
				}
				return new CardCondition(conditionList);
			}
			else
			{
				var conditionString = rawConditionString.Trim().Substring(1).ToLower();
				if (_symbolToNumber.TryGetValue(conditionString, out var condition))
					return new CardCondition(condition);
				else
					throw new Exception($"CardManager::ParseCondition - symbol of {rawConditionString} is not parsable");
			}
		}
		
		/// <summary>
		/// 카드 변수를 파싱한다
		/// </summary>
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
				
				//최대, 최소값이 없을때 
				if (variableNameAndRange.Length == 1)
				{
					variableName = variableNameAndRange[0];
				} 
				//최대, 최소값이 있을때
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
				
				//초기값이 있을때
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
		
		/// <summary>
		/// 카드 효과를 파싱한다
		/// </summary>
		private static CardEffect ParseEffect(string effectString, out bool includeDeath)
		{
			string[] effectModuleStringList = SplitEffectModule(effectString);
			var cardEffectModules = new List<CardEffectModule>();
			
			includeDeath = false;
			if (effectModuleStringList.Contains("Death()"))
				includeDeath = true;
			
			foreach (var effectModuleString in effectModuleStringList)
			{
				// 이펙트 모듈 이름 파싱
				int delimiterIndex = effectModuleString.IndexOf('(');
				//모듈이 아닐경우 무시하고 지나감
				if (delimiterIndex < 0)
					continue;
				string moduleName = effectModuleString.Substring(0, delimiterIndex);

				if (!EffectModuleManager.TryGet(moduleName, out var module, out var type))
					throw new Exception($"CardManager::ParseEffect - {moduleName} is not parsable on module");
				
				// 이펙 모듈 파라미터 파싱
				string parameterListString = effectModuleString.Substring(delimiterIndex);
				var parameterStringSplit = SplitEffectParameter(parameterListString);

				if (parameterStringSplit is [""])
					parameterStringSplit = Array.Empty<string>();
				
				var parameters = new List<CardEffectModule.Parameter>();
				foreach (var parameterString in parameterStringSplit)
				{
					char identifier = parameterString[0];
					
					//Boolean 일때
					if (parameterString is "true" or "false")
					{
						parameters.Add(new CardEffectModule.Parameter(bool.Parse(parameterString)));
					}
					//String일때
					else if (identifier is '\"' or '\'')
					{
						var parameter = parameterString.Substring(1, parameterString.Length - 2);
						parameters.Add(new CardEffectModule.Parameter(parameter, CardEffectModule.Parameter.Type.Data));
					}
					//Number일때
					else if (double.TryParse(parameterString, out var parameter))
					{
						parameters.Add(new CardEffectModule.Parameter(parameter));
					}
					//ModuleList일 때
					else if (identifier == '{')
					{
						var effectListString = parameterString.Substring(1, parameterString.Length - 2);
						var effectInEffect = ParseEffect(effectListString, out _);
						parameters.Add(new CardEffectModule.Parameter(effectInEffect));
					}
					//Module일때
					else if (Regex.IsMatch(parameterString,"[a-zA-Z]*[(].*[)]"))
					{
						var effectInEffect = ParseEffect(parameterString, out _);
						parameters.Add(new CardEffectModule.Parameter(effectInEffect));
					}
					//Number List일때
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
						parameters.Add(new CardEffectModule.Parameter(numberList));
					}
					//Expression일때
					else if (parameterString.Contains('$') || parameterString.Contains('%'))
					{
						parameters.Add(new CardEffectModule.Parameter(parameterString, CardEffectModule.Parameter.Type.Expression));
					}
				}

				cardEffectModules.Add(new CardEffectModule(module, parameters, type));
			}

			if (cardEffectModules.Count == 0)
				return CardEffect.Nothing();
			return new CardEffect(cardEffectModules);
			
			string[] SplitEffectModule(string moduleString)
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
			
			string[] SplitEffectParameter(string parameterString)
			{
				string stringSplitByAt = "";
				int cntEffectDelimiter = 0;
				//() 때기			
				parameterString = parameterString.Substring(1, parameterString.Length - 2);
				//가장 바깥쪽 , 전부 @로 변환
				for (int i = 0; i < parameterString.Length; i++)
				{
					char c = parameterString[i];
					if (c is '[' or '(' or '{') cntEffectDelimiter++;
					if (c is ']' or ')' or '}') cntEffectDelimiter--;

					if (c == ',' && cntEffectDelimiter == 0)
						stringSplitByAt += "@";
					else
						stringSplitByAt += c;
				}

				return stringSplitByAt.Split('@', StringSplitOptions.TrimEntries);
			}
		}

	}
}