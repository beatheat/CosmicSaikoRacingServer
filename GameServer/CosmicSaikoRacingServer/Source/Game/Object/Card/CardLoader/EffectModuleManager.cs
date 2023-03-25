
namespace CSRServer.Game
{
	using ParameterList =  CardEffectModule.ParameterList;
	using Result = CardEffectModule.Result;
	using Type = CardEffectModule.Type;

	public delegate CardEffectModule.Result EffectModule(Card card , GamePlayer player, ParameterList parameters);

	/// <summary>
	/// 카드 이펙트 모듈 로딩 클래스
	/// </summary>
	internal static class EffectModuleManager
	{
		private static readonly Dictionary<string, EffectModule> effectModules = new Dictionary<string, EffectModule>();
		private static readonly Dictionary<string, Type> effectTypes = new Dictionary<string, Type>();
		private static bool _isLoaded = false;

		public static void Load()
		{
			if (_isLoaded) return;
			
			AddModule("Nothing", Nothing, Type.Nothing);
			AddModule("Add", Add, Type.Add);
			AddModule("Multiply", Multiply, Type.Multiply);
			AddModule("Draw", Draw, Type.Draw);
			AddModule("RerollCountUp", RerollCountUp, Type.RerollCountUp);
			AddModule("Death", Death, Type.Death);
			AddModule("Fail", Fail, Type.Fail);
			AddModule("Initialize", Initialize, Type.Initialize);
			AddModule("ForceReroll", ForceReroll, Type.ForceReroll);
			AddModule("CreateCardToHand", CreateCardToHand, Type.CreateCardToHand);
			AddModule("CreateCardToDeck", CreateCardToDeck, Type.CreateCardToDeck);
			AddModule("CreateCardToOther", CreateCardToOther, Type.CreateCardToOther);
			AddModule("BuffToMe", BuffToMe, Type.BuffToMe);
			AddModule("BuffToOther", BuffToOther, Type.BuffToOther);
			AddModule("DoPercent", DoPercent, Type.DoPercent);
			AddModule("SetVariable", SetVariable, Type.SetVariable);
			AddModule("Overload", Overload, Type.Overload);
			AddModule("EraseBuff", EraseBuff, Type.EraseBuff);
			AddModule("Combo", Combo, Type.Combo);
			AddModule("EnforceSelf", EnforceSelf, Type.EnforceSelf);
			AddModule("Discard", Discard, Type.Discard);
			AddModule("Choice", Choice, Type.Choice);
			AddModule("Check", Check, Type.Check);
			AddModule("Leak", Leak, Type.Leak);
			AddModule("Repeat", Repeat, Type.Repeat);
			AddModule("TurnEnd", TurnEnd, Type.TurnEnd);


			_isLoaded = true;
		}
		
		/// <summary>
		/// 초기화시 모듈메소드를 모듈이름과 매핑
		/// </summary>
		private static void AddModule(string moduleName, EffectModule effectModule, Type effectType)
		{
			effectModules.Add(moduleName, effectModule);
			effectTypes.Add(moduleName, effectType);
		}

		/// <summary>
		/// 모듈 이름을 통해 모듈을 가져온다
		/// </summary>
		public static bool TryGet(string moduleName, out EffectModule module, out Type type)
		{
			if (effectModules.TryGetValue(moduleName, out module!))
			{
				type = effectTypes[moduleName];
				return true;
			}

			type = default(Type);
			return false;
		}

		#region Effect Modules


		private static Result Nothing(Card card, GamePlayer player, ParameterList parameters)
		{
			return new Result{result = null, type = Type.Nothing};
		}

		private static Result Add(Card card, GamePlayer player, ParameterList parameters)
		{
			int amount = parameters.Get<int>(0, card, player);
			player.turnDistance += amount;
			return new Result{result = amount, type = Type.Add};;
		}
		
		private static Result Multiply(Card card, GamePlayer player, ParameterList parameters)
		{
			double amount = parameters.Get<double>(0, card, player);
			player.turnDistance = (int)(player.turnDistance * amount);
			return new Result{result = amount, type = Type.Multiply};;
		}

		private static Result Draw(Card card, GamePlayer player, ParameterList parameters)
		{
			int amount = parameters.Get<int>(0, card, player);
			return new Result{result = player.DrawCard(amount), type = Type.Draw};
		}
		
		private static Result RerollCountUp(Card card, GamePlayer player, ParameterList parameters)
		{
			int amount = parameters.Get<int>(0, card, player);
			player.resourceRerollCount += amount;
			return new Result{result = amount, type = Type.RerollCountUp};
		}
		
		private static Result Death(Card card, GamePlayer player, ParameterList parameters)
		{
			card.death = true;
			return new Result{result = null, type = Type.Death};
		}

		private static Result Fail(Card card, GamePlayer player, ParameterList parameters)
		{
			card.enable = false;
			return new Result{result = null, type = Type.Fail};
		}
		
		private static Result Initialize(Card card, GamePlayer player, ParameterList parameters)
		{
			player.turnDistance = 0;
			return new Result{result = null, type = Type.Initialize};
		}
		
		private static Result ForceReroll(Card card, GamePlayer player, ParameterList parameters)
		{
			for (int i = 0; i < player.resourceReelCount; i++)
			{
				player.resourceReel[i] = Util.GetRandomEnumValue<Resource.Type>();
			}
			return new Result{result = player.resourceReel, type = Type.ForceReroll};
		}

		private static Result CreateCardToHand(Card card, GamePlayer player, ParameterList parameters)
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
			return new Result{result = createdCards, type = Type.CreateCardToHand};
		}
		
		private static Result CreateCardToDeck(Card card, GamePlayer player, ParameterList parameters)
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
			return new Result{result = createdCards, type = Type.CreateCardToDeck};
		}
		
		private static Result CreateCardToOther(Card card, GamePlayer player, ParameterList parameters)
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

			Result CreateToOther()
			{
				foreach (int idx in targetPlayerIndex)
                {
                	player.parent[idx].AddCardToDeck(createdCards);
                }
				return new Result{result = result, type = Type.CreateCardToOther};
			}

			if (targetPlayerIndex.Count == 0)
				return Nothing(card, player, parameters);
			
			player.AddDepartEvent(CreateToOther);
			return new Result{result = result, type = Type.CreateCardToOther};
		}	
		
		private static Result BuffToMe(Card card, GamePlayer player, ParameterList parameters)
		{
			int id = parameters.Get<int>(0, card, player);
			int amount = parameters.Get<int>(1, card, player);

			player.AddBuff((Buff.Type) id, amount);
			var result = new Dictionary<string, object> {["buffType"] = (Buff.Type) id, ["count"] = amount};
			return new Result {result = result, type = Type.BuffToMe};
		}	
		
		private static Result BuffToOther(Card card, GamePlayer player, ParameterList parameters)
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

			Result _BuffToOther()
			{
				foreach (int idx in targetPlayerIndex)
				{
					player.parent[idx].AddBuff((Buff.Type)idx, amount);
				}
				return new Result{result = result, type = Type.BuffToOther};
			}

			if (targetPlayerIndex.Count == 0)
			{
				return Nothing(card, player, parameters);
			}
			player.AddDepartEvent(_BuffToOther);
			return new Result{result = result, type = Type.BuffToOther};
		}	
		
		private static Result EraseBuff(Card card, GamePlayer player, ParameterList parameters)
		{
			int id = parameters.Get<int>(0, card, player);
			CardEffect effect = parameters.Get<CardEffect>(1, card, player) ?? CardEffect.Nothing();
			List<Result[]> results = new List<Result[]>();
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
			return new Result{result = results, type = Type.EraseBuff};
		}

		private static Result EnforceSelf(Card card, GamePlayer player, ParameterList parameters)
		{
			// int id = parameters[0].Get<int>(player);
			// int amount = parameters[1].Get<int>(player);
			return new Result{result = null, type = Type.EnforceSelf};
		}

		private static Result DoPercent(Card card, GamePlayer player, ParameterList parameters)
		{
			string percent = parameters.Get<string>(0, card, player) ?? "";
			CardEffect effect = parameters.Get<CardEffect>(1, card, player) ?? CardEffect.Nothing();
			Result[] result = Array.Empty<Result>();
			
			
			if (card.variable.ContainsKey(percent))
			{
				var _percent = card.variable[percent];
				Random random = new Random();
				if (random.Next(100) < _percent.value)
				{
					result = effect.Use(card, player);
				}
			}
			return new Result{result = result, type = Type.DoPercent};
		}	
		
		private static Result SetVariable(Card card, GamePlayer player, ParameterList parameters)
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

			return new Result{result = null, type = Type.SetVariable};
		}	
		
		private static Result Overload(Card card, GamePlayer player, ParameterList parameters)
		{
			string percent = parameters.Get<string>(0, card, player) ?? "";
			int amount = parameters.Get<int>(1, card, player);
			CardEffect effect = parameters.Get<CardEffect>(2, card, player) ?? CardEffect.Nothing();;
			
			Result[] result = Array.Empty<Result>();
			
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
			return new Result{result = result, type = Type.DoPercent};
		}	

		
		private static Result Combo(Card card, GamePlayer player, ParameterList parameters)
		{
			List<int> idList = parameters.Get<List<int>>(0, card, player) ?? new List<int> {0};
			CardEffect effect = parameters.Get<CardEffect>(1, card, player) ?? CardEffect.Nothing();

			bool isComboReady = true;
			foreach (var id in idList)
			{
				Card? find = player.turnUsedCard.Find(x => x.id == id);
				isComboReady = isComboReady && (find != null);
			}

			Result[] result = Array.Empty<Result>();
			if (isComboReady)
			{
				result = effect.Use(card, player);
			}
			return new Result{result = result, type = Type.Combo};
		}

		private static Result Discard(Card card, GamePlayer player, ParameterList parameters)
		{
			int amount = parameters.Get<int>(0, card, player);
			CardEffect effect = parameters.Get<CardEffect>(1, card, player) ?? CardEffect.Nothing();
			Random random = new Random();
			
			if (amount > player.hand.Count)
				amount = player.hand.Count();

			List<Result[]> leakResults = new List<Result[]>();
			List<Result[]> discardResults = new List<Result[]>();
			List<int> throwIndexList = new List<int>();
			//amount장의 카드를 버리고 leak효과+ discard특수효과를 발동한다.
			for (int i = 0; i < amount; i++)
			{
				int throwIndex = random.Next(player.hand.Count);
				throwIndexList.Add(throwIndex);
				Result[] throwResult = player.ThrowCard(throwIndex);
				leakResults.Add(throwResult);
				Result[] discardResult = effect.Use(card, player);
				discardResults.Add(discardResult);
			}

			var result = new Dictionary<string, object>
			{
				["throwIndexList"] = throwIndexList,
				["leakResults"] = leakResults,
				["discardResults"] = discardResults
			};
			
			return new Result{result = result, type = Type.Discard};
		}	
		
		private static Result Choice(Card card, GamePlayer player, ParameterList parameters)
		{
			CardEffect effect = parameters.Get<CardEffect>(0, card, player) ?? CardEffect.Nothing();
			Random random = new Random();
			var result = effect.Use(random.Next(effect.count), card, player);

			return new Result{result = result, type = Type.Choice};
		}	
		
		private static Result Check(Card card, GamePlayer player, ParameterList parameters)
		{
			bool condition = parameters.Get<bool>(0, card, player);
			CardEffect effect = parameters.Get<CardEffect>(1, card, player) ?? CardEffect.Nothing();

			Result[] result = {Nothing(card, player, parameters)};
			if(condition == true)
				result = effect.Use(card, player);

			return new Result{result = result, type = Type.Check};
		}	
		
		private static Result Leak(Card card, GamePlayer player, ParameterList parameters)
		{
			CardEffect effect = parameters.Get<CardEffect>(0, card, player) ?? CardEffect.Nothing();
			var result = effect.Use(card, player);
			return new Result{result = result, type = Type.Leak};
		}	
		
		private static Result Repeat(Card card, GamePlayer player, ParameterList parameters)
		{
			int amount = parameters.Get<int>(0, card, player);
			CardEffect effect = parameters.Get<CardEffect>(1, card, player) ?? CardEffect.Nothing();
			
			List<Result[]> results = new List<Result[]>();
			for (int i = 0; i < amount; i++)
				results.Add(effect.Use(card, player));
			return new Result{result = results, type = Type.Repeat};		
		}	
		
		private static Result TurnEnd(Card card, GamePlayer player, ParameterList parameters)
		{
			card.enable = false;
			player.scene.preheatPhase.Ready(player);
			return new Result{result = null, type = Type.TurnEnd};
		}
		#endregion
	}
}