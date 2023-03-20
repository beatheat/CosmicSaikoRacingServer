
namespace CSRServer.Game
{
	using ParameterList =  CardEffect.ParameterList;

	public delegate CardEffect.Result EffectModule(Card card , GamePlayer player, ParameterList parameters);
	
	internal static class EffectModuleManager
	{
		private static readonly Dictionary<string, EffectModule> effectModules = new Dictionary<string, EffectModule>();
		private static readonly Dictionary<string, CardEffect.Type> effectTypes = new Dictionary<string, CardEffect.Type>();
		private static bool _isLoaded = false;

		public static void Load()
		{
			if (_isLoaded) return;
			
			AddModule("Nothing", Nothing, CardEffect.Type.Nothing);
			AddModule("Add", Add, CardEffect.Type.Add);
			AddModule("Multiply", Multiply, CardEffect.Type.Multiply);
			AddModule("Draw", Draw, CardEffect.Type.Draw);
			AddModule("RerollCountUp", RerollCountUp, CardEffect.Type.RerollCountUp);
			AddModule("Death", Death, CardEffect.Type.Death);
			AddModule("Fail", Fail, CardEffect.Type.Fail);
			AddModule("Initialize", Initialize, CardEffect.Type.Initialize);
			AddModule("ForceReroll", ForceReroll, CardEffect.Type.ForceReroll);
			AddModule("CreateCardToHand", CreateCardToHand, CardEffect.Type.CreateCardToHand);
			AddModule("CreateCardToDeck", CreateCardToDeck, CardEffect.Type.CreateCardToDeck);
			AddModule("CreateCardToOther", CreateCardToOther, CardEffect.Type.CreateCardToOther);
			AddModule("BuffToMe", BuffToMe, CardEffect.Type.BuffToMe);
			AddModule("BuffToOther", BuffToOther, CardEffect.Type.BuffToOther);
			AddModule("DoPercent", DoPercent, CardEffect.Type.DoPercent);
			AddModule("SetVariable", SetVariable, CardEffect.Type.SetVariable);
			AddModule("Overload", Overload, CardEffect.Type.Overload);
			AddModule("EraseBuff", EraseBuff, CardEffect.Type.EraseBuff);
			AddModule("Combo", Combo, CardEffect.Type.Combo);
			AddModule("EnforceSelf", EnforceSelf, CardEffect.Type.EnforceSelf);
			AddModule("Discard", Discard, CardEffect.Type.Discard);
			AddModule("Choice", Choice, CardEffect.Type.Choice);
			AddModule("Check", Check, CardEffect.Type.Check);
			AddModule("Leak", Leak, CardEffect.Type.Leak);
			AddModule("Repeat", Repeat, CardEffect.Type.Repeat);
			AddModule("TurnEnd", TurnEnd, CardEffect.Type.TurnEnd);


			_isLoaded = true;
		}

		private static void AddModule(string moduleName, EffectModule effectModule, CardEffect.Type effectType)
		{
			effectModules.Add(moduleName, effectModule);
			effectTypes.Add(moduleName, effectType);
		}

		public static bool TryGet(string moduleName, out EffectModule module, out CardEffect.Type type)
		{
			if (effectModules.TryGetValue(moduleName, out module!))
			{
				type = effectTypes[moduleName];
				return true;
			}

			type = default(CardEffect.Type);
			return false;
		}

		//Effect Modules
		private static CardEffect.Result Nothing(Card card, GamePlayer player, ParameterList parameters)
		{
			return new CardEffect.Result{result = null, type = CardEffect.Type.Nothing};
		}

		private static CardEffect.Result Add(Card card, GamePlayer player, ParameterList parameters)
		{
			int amount = parameters.Get<int>(0, card, player);
			player.turnDistance += amount;
			return new CardEffect.Result{result = amount, type = CardEffect.Type.Add};;
		}
		
		private static CardEffect.Result Multiply(Card card, GamePlayer player, ParameterList parameters)
		{
			double amount = parameters.Get<double>(0, card, player);
			player.turnDistance = (int)(player.turnDistance * amount);
			return new CardEffect.Result{result = amount, type = CardEffect.Type.Multiply};;
		}

		private static CardEffect.Result Draw(Card card, GamePlayer player, ParameterList parameters)
		{
			int amount = parameters.Get<int>(0, card, player);
			return new CardEffect.Result{result = player.DrawCard(amount), type = CardEffect.Type.Draw};
		}
		
		private static CardEffect.Result RerollCountUp(Card card, GamePlayer player, ParameterList parameters)
		{
			int amount = parameters.Get<int>(0, card, player);
			player.resourceRerollCount += amount;
			return new CardEffect.Result{result = amount, type = CardEffect.Type.RerollCountUp};
		}
		
		private static CardEffect.Result Death(Card card, GamePlayer player, ParameterList parameters)
		{
			card.death = true;
			return new CardEffect.Result{result = null, type = CardEffect.Type.Death};
		}

		private static CardEffect.Result Fail(Card card, GamePlayer player, ParameterList parameters)
		{
			card.enable = false;
			return new CardEffect.Result{result = null, type = CardEffect.Type.Fail};
		}
		
		private static CardEffect.Result Initialize(Card card, GamePlayer player, ParameterList parameters)
		{
			player.turnDistance = 0;
			return new CardEffect.Result{result = null, type = CardEffect.Type.Initialize};
		}
		
		private static CardEffect.Result ForceReroll(Card card, GamePlayer player, ParameterList parameters)
		{
			for (int i = 0; i < player.resourceReelCount; i++)
			{
				player.resourceReel[i] = Util.GetRandomEnumValue<Resource.Type>();
			}
			return new CardEffect.Result{result = player.resourceReel, type = CardEffect.Type.ForceReroll};
		}

		private static CardEffect.Result CreateCardToHand(Card card, GamePlayer player, ParameterList parameters)
		{
			int id = parameters.Get<int>(0, card, player);
			int amount = parameters.Get<int>(1, card, player);
			bool isDeath = parameters.Get<bool>(2, card, player);

			Card[] createdCards = new Card[amount];
			for (int i = 0; i < amount; i++)
			{
				createdCards[i] = CardManager.GetCard(id);
				if(isDeath)
					createdCards[i].death = true;
				player.AddCardToHand(createdCards[i]);

			}
			return new CardEffect.Result{result = createdCards, type = CardEffect.Type.CreateCardToHand};
		}
		
		private static CardEffect.Result CreateCardToDeck(Card card, GamePlayer player, ParameterList parameters)
		{
			int id = parameters.Get<int>(0, card, player);
			int amount = parameters.Get<int>(1, card, player);
			bool isDeath = parameters.Get<bool>(3, card, player);

			Card[] createdCards = new Card[amount];
			for (int i = 0; i < amount; i++)
			{
				createdCards[i] = CardManager.GetCard(id);
				if(isDeath)
					createdCards[i].death = true;
				player.AddCardToDeck(createdCards[i]);
			}
			return new CardEffect.Result{result = createdCards, type = CardEffect.Type.CreateCardToDeck};
		}
		
		private static CardEffect.Result CreateCardToOther(Card card, GamePlayer player, ParameterList parameters)
		{
			int id = parameters.Get<int>(0, card, player);
			int amount = parameters.Get<int>(1, card, player);
			int target = parameters.Get<int>(2, card, player);
			bool isDeath = parameters.Get<bool>(3, card, player);


			Card[] createdCards = new Card[amount];
			for (int i = 0; i < amount; i++)
			{
				createdCards[i] = CardManager.GetCard(id);
				if(isDeath)
					createdCards[i].death = true;
			}
			
			List<int> targetPlayerIndex = new List<int>();
			//자신을 제외한 모든 플레이어
			if (target == 0)
			{
				foreach (var p in player.parent)
				{
					if(p != player)
						targetPlayerIndex.Add(p.index);
				}
			}
			// 해당 등수
			else if (target is >= 1 and <= 4)
			{
				var p = player.parent.Find(x => x.rank == target);
				if (p != player)
					targetPlayerIndex.Add(p!.index);
			}
			//자신 바로 앞 플레이어
			else if (target == 5)
			{
				if (player.rank > 1)
				{
					var p = player.parent.Find(x => x.rank == player.rank - 1);
					targetPlayerIndex.Add(p!.index);
				}
			}
			//자신 바로 뒤 플레이어
			else if (target == 6)
			{
				if (player.rank < 4)
				{
					var p = player.parent.Find(x => x.rank == player.rank + 1);
					targetPlayerIndex.Add(p!.index);
				}
			}
			// target->distance 범위 안의 플레이어
			else if (target >= 7)
			{
				int distance = target;
				foreach (var p in player.parent)
				{
					if (Math.Abs(p.currentDistance - player.currentDistance) <= distance)
						targetPlayerIndex.Add(p.index);
				}
			}
			
			var result = new Dictionary<string, object>
			{
				["srcIndex"] = player.index,
				["dstIndexList"] = targetPlayerIndex,
				["createdCards"] = createdCards
			};

			CardEffect.Result CreateToOther()
			{
				foreach (int idx in targetPlayerIndex)
                {
                	player.parent[idx].AddCardToDeck(createdCards);
                }
				return new CardEffect.Result{result = result, type = CardEffect.Type.CreateCardToOther};
			}

			if (targetPlayerIndex.Count == 0)
				return Nothing(card, player, parameters);
			
			player.AddDepartEvent(CreateToOther);
			return new CardEffect.Result{result = result, type = CardEffect.Type.CreateCardToOther};
		}	
		
		private static CardEffect.Result BuffToMe(Card card, GamePlayer player, ParameterList parameters)
		{
			int id = parameters.Get<int>(0, card, player);
			int amount = parameters.Get<int>(1, card, player);

			player.AddBuff((Buff.Type) id, amount);
			var result = new Dictionary<string, object> {["buffType"] = (Buff.Type) id, ["count"] = amount};
			return new CardEffect.Result {result = result, type = CardEffect.Type.BuffToMe};
		}	
		
		private static CardEffect.Result BuffToOther(Card card, GamePlayer player, ParameterList parameters)
		{
			int id = parameters.Get<int>(0, card, player);
			int amount = parameters.Get<int>(1,card, player);
			int target = parameters.Get<int>(2,card, player);
			

			List<int> targetPlayerIndex = new List<int>();
            //자신을 제외한 모든 플레이어
            if (target == 0)
            {
            	foreach (var p in player.parent)
            	{
            		if(p != player)
            			targetPlayerIndex.Add(p.index);
            	}
            }
            // 해당 등수
            else if (target is >= 1 and <= 4)
            {
            	var p = player.parent.Find(x => x.rank == target);
            	if (p != player)
            		targetPlayerIndex.Add(p!.index);
            }
            //자신 바로 앞 플레이어
            else if (target == 5)
            {
            	if (player.rank > 1)
            	{
            		var p = player.parent.Find(x => x.rank == player.rank - 1);
            		targetPlayerIndex.Add(p!.index);
            	}
            }
            //자신 바로 뒤 플레이어
            else if (target == 6)
            {
            	if (player.rank < 4)
            	{
            		var p = player.parent.Find(x => x.rank == player.rank + 1);
            		targetPlayerIndex.Add(p!.index);
            	}
            }
            // target->distance 범위 안의 플레이어
            else if (target >= 7)
            {
            	int distance = target;
            	foreach (var p in player.parent)
            	{
            		if (Math.Abs(p.currentDistance - player.currentDistance) <= distance)
            			targetPlayerIndex.Add(p.index);
            	}
            }
 
            var result = new Dictionary<string, object>
            {
            	["srcIndex"] = player.index,
            	["dstIndexList"] = targetPlayerIndex,
            	["buffType"] = (Buff.Type)id,
                ["amount"] = amount
            };

			CardEffect.Result _BuffToOther()
			{
				foreach (int idx in targetPlayerIndex)
				{
					player.parent[idx].AddBuff((Buff.Type)idx, amount);
				}
				return new CardEffect.Result{result = result, type = CardEffect.Type.BuffToOther};
			}

			if (targetPlayerIndex.Count == 0)
			{
				return Nothing(card, player, parameters);
			}
			player.AddDepartEvent(_BuffToOther);
			return new CardEffect.Result{result = result, type = CardEffect.Type.BuffToOther};
		}	
		
		private static CardEffect.Result EraseBuff(Card card, GamePlayer player, ParameterList parameters)
		{
			int id = parameters.Get<int>(0, card, player);
			CardEffect effect = parameters.Get<CardEffect>(1, card, player) ?? CardEffect.Nothing();
			List<CardEffect.Result[]> results = new List<CardEffect.Result[]>();
			//모든 버프 제거
			if (id == 99)
			{
				int amount = player.buffManager.ReleaseAll();
				for (int i = 0; i < amount; i++)
					results.Add(effect.Use(card, player));
			}
			//특정버프제거
			else
			{
				int amount = player.buffManager.Release((Buff.Type) id);
				for (int i = 0; i < amount; i++)
					results.Add(effect.Use(card, player));

			}
			return new CardEffect.Result{result = results, type = CardEffect.Type.EraseBuff};
		}

		private static CardEffect.Result EnforceSelf(Card card, GamePlayer player, ParameterList parameters)
		{
			// int id = parameters[0].Get<int>(player);
			// int amount = parameters[1].Get<int>(player);
			return new CardEffect.Result{result = null, type = CardEffect.Type.EnforceSelf};
		}

		private static CardEffect.Result DoPercent(Card card, GamePlayer player, ParameterList parameters)
		{
			string percent = parameters.Get<string>(0, card, player) ?? "";
			CardEffect effect = parameters.Get<CardEffect>(1, card, player) ?? CardEffect.Nothing();
			CardEffect.Result[] result = Array.Empty<CardEffect.Result>();
			
			
			if (card.variable.ContainsKey(percent))
			{
				var _percent = card.variable[percent];
				Random random = new Random();
				if (random.Next(100) < _percent.value)
				{
					result = effect.Use(card, player);
				}
			}
			return new CardEffect.Result{result = result, type = CardEffect.Type.DoPercent};
		}	
		
		private static CardEffect.Result SetVariable(Card card, GamePlayer player, ParameterList parameters)
		{
			string varName = parameters.Get<string>(0, card, player) ?? "";
			int number = parameters.Get<int>(1, card, player);

			if (card.variable.ContainsKey(varName))
			{
				var variable = card.variable[varName];
				variable.value += number;
				if (variable.value < variable.lowerBound)
					variable.value = variable.lowerBound;
				if (variable.value > variable.upperBound)
					variable.value = variable.upperBound;
			}

			return new CardEffect.Result{result = null, type = CardEffect.Type.SetVariable};
		}	
		
		private static CardEffect.Result Overload(Card card, GamePlayer player, ParameterList parameters)
		{
			string percent = parameters.Get<string>(0, card, player) ?? "";
			int amount = parameters.Get<int>(1, card, player);
			CardEffect effect = parameters.Get<CardEffect>(2, card, player) ?? CardEffect.Nothing();;
			
			CardEffect.Result[] result = Array.Empty<CardEffect.Result>();
			
			if (card.variable.ContainsKey(percent))
			{
				var _percent = card.variable[percent];
				Random random = new Random();
				if (random.Next(100) < _percent.value)
				{
					result = effect.Use(card, player);
				}
				_percent.value += amount;
				if (_percent.value < _percent.lowerBound)
					_percent.value = _percent.lowerBound;
				if (_percent.value > _percent.upperBound)
					_percent.value = _percent.upperBound;
			}
			return new CardEffect.Result{result = result, type = CardEffect.Type.DoPercent};
		}	

		
		private static CardEffect.Result Combo(Card card, GamePlayer player, ParameterList parameters)
		{
			List<int> idList = parameters.Get<List<int>>(0, card, player) ?? new List<int> {0};
			CardEffect effect = parameters.Get<CardEffect>(1, card, player) ?? CardEffect.Nothing();;

			bool isComboReady = true;
			foreach (var id in idList)
			{
				Card? find = player.turnUsedCard.Find(x => x.id == id);
				isComboReady = isComboReady && (find != null);
			}

			CardEffect.Result[] result = Array.Empty<CardEffect.Result>();
			if (isComboReady)
			{
				result = effect.Use(card, player);
			}
			return new CardEffect.Result{result = result, type = CardEffect.Type.Combo};
		}

		private static CardEffect.Result Discard(Card card, GamePlayer player, ParameterList parameters)
		{
			int amount = parameters.Get<int>(0, card, player);
			CardEffect effect = parameters.Get<CardEffect>(1, card, player) ?? CardEffect.Nothing();
			Random random = new Random();
			
			if (amount > player.hand.Count)
				amount = player.hand.Count();

			List<CardEffect.Result[]> leakResults = new List<CardEffect.Result[]>();
			List<CardEffect.Result[]> discardResults = new List<CardEffect.Result[]>();
			List<int> throwIndexList = new List<int>();
			//amount장의 카드를 버린다.
			for (int i = 0; i < amount; i++)
			{
				int throwIndex = random.Next(player.hand.Count);
				throwIndexList.Add(throwIndex);
				CardEffect.Result[] throwResult = player.ThrowCard(throwIndex);
				leakResults.Add(throwResult);
			}
			//amount번의 특수효과를 발동한다.
			for (int i = 0; i < amount; i++)
			{
				CardEffect.Result[] discardResult = effect.Use(card, player);
				discardResults.Add(discardResult);
			}

			var result = new Dictionary<string, object>
			{
				["throwIndexList"] = throwIndexList,
				["leakResults"] = leakResults,
				["discardResults"] = discardResults
			};
			
			return new CardEffect.Result{result = result, type = CardEffect.Type.Discard};
		}	
		
		private static CardEffect.Result Choice(Card card, GamePlayer player, ParameterList parameters)
		{
			CardEffect effect = parameters.Get<CardEffect>(0, card, player) ?? CardEffect.Nothing();
			Random random = new Random();
			var result = effect.Use(random.Next(effect.ElementCount), card, player);

			return new CardEffect.Result{result = result, type = CardEffect.Type.Choice};
		}	
		
		private static CardEffect.Result Check(Card card, GamePlayer player, ParameterList parameters)
		{
			bool condition = parameters.Get<bool>(0, card, player);
			CardEffect effect = parameters.Get<CardEffect>(1, card, player) ?? CardEffect.Nothing();

			CardEffect.Result[] result = {Nothing(card, player, parameters)};
			if(condition == true)
				result = effect.Use(card, player);

			return new CardEffect.Result{result = result, type = CardEffect.Type.Check};
		}	
		
		private static CardEffect.Result Leak(Card card, GamePlayer player, ParameterList parameters)
		{
			CardEffect effect = parameters.Get<CardEffect>(0, card, player) ?? CardEffect.Nothing();
			var result = effect.Use(card, player);
			return new CardEffect.Result{result = result, type = CardEffect.Type.Leak};
		}	
		
		private static CardEffect.Result Repeat(Card card, GamePlayer player, ParameterList parameters)
		{
			int amount = parameters.Get<int>(0, card, player);
			CardEffect effect = parameters.Get<CardEffect>(1, card, player) ?? CardEffect.Nothing();
			
			List<CardEffect.Result[]> results = new List<CardEffect.Result[]>();
			for (int i = 0; i < amount; i++)
				results.Add(effect.Use(card, player));
			return new CardEffect.Result{result = results, type = CardEffect.Type.Repeat};		
		}	
		
		private static CardEffect.Result TurnEnd(Card card, GamePlayer player, ParameterList parameters)
		{
			card.enable = false;
			player.turnReady = true;
			return new CardEffect.Result{result = null, type = CardEffect.Type.TurnEnd};
		}	
	}
}