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
			["yatch"] = 5
		};
		
		public static void Load(string path)
		{
			if (_isLoaded) return;
			Logger.Log("Card effect module is loading...");
			CardEffectModule.Load();
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
				var jsonCondition = jsonCard["condition"];
				var jsonEffect = jsonCard["effect"];

				int id;
				int rank;
				Card.Type type;
				CardCondition condition;
				CardEffect effect;
				
				if (jsonId == null || !int.TryParse(jsonId.ToString(), out id))
					throw new Exception($"CardManager::Load - index({i}) : id attribute is missing");

				if(jsonRank == null || !int.TryParse(jsonRank.ToString(), out  rank))
					throw new Exception($"CardManager::Load - index({i}) : rank attribute is missing");
				
				if(jsonType == null || !symbolToCardType.TryGetValue(jsonType.ToString(), out type)) 
					throw new Exception($"CardManager::Load - index({i}) : type attribute is missing");
				
				if (jsonCondition != null)
					condition = ParseCondition(jsonCondition.ToString());
				else 
					throw new Exception($"CardManager::Load - index({i}) : condition attribute is missing");
				
				if(jsonEffect != null)
					effect = ParseEffect(jsonEffect.ToString());
				else
					throw new Exception($"CardManager::Load - index({i}) : effect attribute is missing");

				cards.Add(new Card(id, type, rank, condition, effect));
			}
			Logger.LogWithClear("Card data parsing ends");
			_isLoaded = true;
		}

		public static Card CloneCard(int index)
		{
			if(index >= 0 && index < cards.Count)
				return cards[index].Clone();
			return null!;
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

				if (!CardEffectModule.TryGet(moduleName, out var module, out var type))
					throw new Exception($"CardManager::ParseEffect - {effectString} is not parsable on module");
				
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
					if (Char.IsDigit(identifier))
					{
						if(double.TryParse(parameterString,out var parameter))
							parameters.Add(new CardEffect.Parameter(parameter));
						else
							throw new Exception($"CardManager::ParseEffect - {effectString} is not parsable on number : {parameterString}");
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
					else if (identifier == '{')
					{
						var effectListString = parameterString.Substring(1, parameterString.Length - 2);
						var effectInEffect = ParseEffect(effectListString);
						parameters.Add(new CardEffect.Parameter(effectInEffect));
					}
					else if (identifier == '%')
					{
						var varParameterString = parameterString.Substring(1);
						parameters.Add(new CardEffect.Parameter(varParameterString, true));
					}
				}

				cardEffectElements.Add(new CardEffect.Element(module, parameters, type));
			}
			return new CardEffect(cardEffectElements);
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
			int cntEffectDelimeter = 0;
			//() 때기			
			parameterString = parameterString.Substring(1, parameterString.Length - 2);
			//가장 바깥쪽 , 전부 @로 변환
			for (int i = 0; i < parameterString.Length; i++)
			{
				char c = parameterString[i];
				if (c == '[') isInNumList = true;
				if (c == ']') isInNumList = false;

				if (c == '{') cntEffectDelimeter++;
				if (c == '}') cntEffectDelimeter--;
				
				if (c == ',' && !isInNumList && cntEffectDelimeter == 0)
					stringSplitByAt += "@";
				else
					stringSplitByAt += c;
			}

			return stringSplitByAt.Split('@', StringSplitOptions.TrimEntries);
		}
	}
}