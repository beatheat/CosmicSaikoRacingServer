
namespace CSRServer.Game
{
	using ParameterList = List<CardEffect.Parameter>;
	internal delegate CardEffect.Result EffectModule(Card card , GamePlayer player, List<CardEffect.Parameter> parameters);
	
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
			AddModule("MountCard", MountCard, CardEffect.Type.MountCard);
			AddModule("MountBuff", MountBuff, CardEffect.Type.MountBuff);
			AddModule("EraseBuff", EraseBuff, CardEffect.Type.EraseBuff);
			AddModule("Combo", Combo, CardEffect.Type.Combo);
			AddModule("EnforceSelf", EnforceSelf, CardEffect.Type.EnforceSelf);
			AddModule("Discard", Discard, CardEffect.Type.Discard);
			AddModule("Choice", Choice, CardEffect.Type.Choice);
			
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
			int amount = parameters[0].Get<int>(card, player);
			player.turnDistance += amount;
			return new CardEffect.Result{result = amount, type = CardEffect.Type.Add};;
		}
		
		private static CardEffect.Result Multiply(Card card, GamePlayer player, ParameterList parameters)
		{
			double amount = parameters[0].Get<double>(card, player);
			player.turnDistance = (int)(player.turnDistance * amount);
			return new CardEffect.Result{result = amount, type = CardEffect.Type.Multiply};;
		}

		private static CardEffect.Result Draw(Card card, GamePlayer player, ParameterList parameters)
		{
			int amount = parameters[0].Get<int>(card, player);
			return new CardEffect.Result{result = player.DrawCard(amount), type = CardEffect.Type.Draw};
		}
		
		private static CardEffect.Result RerollCountUp(Card card, GamePlayer player, ParameterList parameters)
		{
			int amount = parameters[0].Get<int>(card, player);
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
			for (int i = 0; i < player.resourceCount; i++)
			{
				player.resourceReel[i] = Util.GetRandomEnumValue<ResourceType>();
			}
			return new CardEffect.Result{result = player.resourceReel, type = CardEffect.Type.ForceReroll};
		}

		private static CardEffect.Result CreateCardToHand(Card card, GamePlayer player, ParameterList parameters)
		{
			int id = parameters[0].Get<int>(card, player);
			int amount = parameters[1].Get<int>(card, player);
			int isDeath = parameters[2].Get<int>(card, player);

			Card[] createdCards = new Card[amount];
			for (int i = 0; i < amount; i++)
			{
				createdCards[i] = CardManager.GetCard(id);
				if(isDeath != 0)
					createdCards[i].death = true;
				player.AddCardToHand(createdCards[i]);

			}
			return new CardEffect.Result{result = createdCards, type = CardEffect.Type.CreateCardToHand};
		}
		
		private static CardEffect.Result CreateCardToDeck(Card card, GamePlayer player, ParameterList parameters)
		{
			int id = parameters[0].Get<int>(card, player);
			int amount = parameters[1].Get<int>(card, player);
			int isDeath = parameters[2].Get<int>(card, player);

			Card[] createdCards = new Card[amount];
			for (int i = 0; i < amount; i++)
			{
				createdCards[i] = CardManager.GetCard(id);
				if(isDeath != 0)
					createdCards[i].death = true;
				player.AddCardToDeck(createdCards[i]);
			}
			return new CardEffect.Result{result = createdCards, type = CardEffect.Type.CreateCardToDeck};
		}
		
		private static CardEffect.Result CreateCardToOther(Card card, GamePlayer player, ParameterList parameters)
		{
			int id = parameters[0].Get<int>(card, player);
			int amount = parameters[1].Get<int>(card, player);
			int isDeath = parameters[2].Get<int>(card, player);
			int target = parameters[3].Get<int>(card, player);

			Card[] createdCards = new Card[amount];
			for (int i = 0; i < amount; i++)
			{
				createdCards[i] = CardManager.GetCard(id);
				if(isDeath != 0)
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
			
			player.AddPreheatEndEvent(CreateToOther);
			return new CardEffect.Result{result = result, type = CardEffect.Type.CreateCardToOther};
		}	
		
		private static CardEffect.Result BuffToMe(Card card, GamePlayer player, ParameterList parameters)
		{
			int id = parameters[0].Get<int>(card, player);
			int amount = parameters[1].Get<int>(card, player);

			player.AddBuff((Buff.Type) id, amount);
			var result = new Dictionary<string, object> {["buffType"] = (Buff.Type) id, ["count"] = amount};
			return new CardEffect.Result {result = result, type = CardEffect.Type.BuffToMe};
		}	
		
		private static CardEffect.Result BuffToOther(Card card, GamePlayer player, ParameterList parameters)
		{
			int id = parameters[0].Get<int>(card, player);
			int amount = parameters[1].Get<int>(card, player);
			int target = parameters[2].Get<int>(card, player);
			

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
            	["source"] = player.index,
            	["destination"] = targetPlayerIndex,
            	["buffs"] = id
            };

			CardEffect.Result _BuffToOther()
			{
				foreach (int idx in targetPlayerIndex)
				{
					player.parent[idx].AddBuff((Buff.Type)id, amount);
				}
				return new CardEffect.Result{result = result, type = CardEffect.Type.BuffToOther};
			}
			
			player.AddPreheatEndEvent(_BuffToOther);
			
			return new CardEffect.Result{result = result, type = CardEffect.Type.BuffToOther};
		}	
		
		private static CardEffect.Result EraseBuff(Card card, GamePlayer player, ParameterList parameters)
		{
			CardEffect effect = parameters[0].Get<CardEffect>(card, player);
			int amount = 0;
			foreach (var buff in player.buffs.Values)
			{
				amount += buff.count;
				buff.count = 0;
			}

			List<CardEffect.Result[]> results = new List<CardEffect.Result[]>();
			for (int i = 0; i < amount; i++)
				results.Add(effect.Use(card, player));
			return new CardEffect.Result{result = results, type = CardEffect.Type.EraseBuff};
		}	
		
		private static CardEffect.Result MountCard(Card card, GamePlayer player, ParameterList parameters)
		{
			int id = parameters[0].Get<int>(card, player);
			int amount = parameters[1].Get<int>(card, player);
			int isDeath = parameters[2].Get<int>(card, player);
			Obstacle obstacle = new Obstacle(Obstacle.Type.CARD, player.currentDistance , id, amount, isDeath == 0);
			player.obstacleList.Add(obstacle);
			
			CardEffect.Result _MountCard()
			{
				return new CardEffect.Result{result = null, type = CardEffect.Type.MountCard};
			}
			
			player.AddPreheatEndEvent(_MountCard);
			
			return new CardEffect.Result{result = null, type = CardEffect.Type.MountCard};
		}	
		
				
		private static CardEffect.Result MountBuff(Card card, GamePlayer player, ParameterList parameters)
		{
			int id = parameters[0].Get<int>(card, player);
			int amount = parameters[1].Get<int>(card, player);

			Obstacle obstacle = new Obstacle(Obstacle.Type.BUFF, player.currentDistance , id, amount);
			player.obstacleList.Add(obstacle);
			
			CardEffect.Result _MountBuff()
            {
            	return new CardEffect.Result{result = null, type = CardEffect.Type.MountBuff};
            }
            
            player.AddPreheatEndEvent(_MountBuff);
			
			return new CardEffect.Result{result = null, type = CardEffect.Type.MountBuff};
		}	
		
		private static CardEffect.Result EnforceSelf(Card card, GamePlayer player, ParameterList parameters)
		{
			// int id = parameters[0].Get<int>(player);
			// int amount = parameters[1].Get<int>(player);
			return new CardEffect.Result{result = null, type = CardEffect.Type.EnforceSelf};
		}

		private static CardEffect.Result DoPercent(Card card, GamePlayer player, ParameterList parameters)
		{
			string percent = parameters[0].Get<string>(card, player);
			CardEffect effect = parameters[1].Get<CardEffect>(card, player);
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
			string varName = parameters[0].Get<string>(card, player);
			int number = parameters[1].Get<int>(card, player);

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
			string percent = parameters[0].Get<string>(card, player);
			int amount = parameters[1].Get<int>(card, player);
			CardEffect effect = parameters[2].Get<CardEffect>(card, player);
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
			List<int> idList = parameters[0].Get<List<int>>(card, player);
			CardEffect effect = parameters[1].Get<CardEffect>(card, player);

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

		//수정필요
		private static CardEffect.Result Discard(Card card, GamePlayer player, ParameterList parameters)
		{
			int amount = parameters[0].Get<int>(card, player);
			CardEffect effect = parameters[1].Get<CardEffect>(card, player);
			Random random = new Random();
			
			if (amount > player.hand.Count)
				amount = player.hand.Count();

			List<CardEffect.Result[]> result = new List<CardEffect.Result[]>();
			for (int i = 0; i < amount; i++)
			{
				player.ThrowCard(random.Next(player.hand.Count));
				result.Add(effect.Use(card, player));
			}

			return new CardEffect.Result{result = result, type = CardEffect.Type.Discard};
		}	
		
		private static CardEffect.Result Choice(Card card, GamePlayer player, ParameterList parameters)
		{
			CardEffect effect = parameters[1].Get<CardEffect>(card, player);
			Random random = new Random();
			var result = effect.Use(random.Next(effect.ElementCount), card, player);

			return new CardEffect.Result{result = result, type = CardEffect.Type.Choice};
		}	
	}
}