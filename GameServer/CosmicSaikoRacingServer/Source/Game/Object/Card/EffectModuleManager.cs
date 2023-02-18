namespace CSRServer.Game
{
	using ParameterList = List<CardEffect.Parameter>;
	internal delegate CardEffect.Result EffectModule(Card card , GamePlayer player, List<CardEffect.Parameter> parameters);
	
	internal class CardEffectManager
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
			int amount = parameters[0].Get<int>(player);
			player.turnDistance += amount;
			return new CardEffect.Result{result = amount, type = CardEffect.Type.Add};;
		}
		
		private static CardEffect.Result Multiply(Card card, GamePlayer player, ParameterList parameters)
		{
			double amount = parameters[0].Get<double>(player);
			player.currentDistance = (int)(player.currentDistance * amount);
			return new CardEffect.Result{result = amount, type = CardEffect.Type.Multiply};;
		}

		private static CardEffect.Result Draw(Card card, GamePlayer player, ParameterList parameters)
		{
			int amount = parameters[0].Get<int>(player);
			return new CardEffect.Result{result = player.DrawCard(amount), type = CardEffect.Type.Draw};
		}
		
		private static CardEffect.Result RerollCountUp(Card card, GamePlayer player, ParameterList parameters)
		{
			int amount = parameters[0].Get<int>(player);
			player.resourceRerollCount += amount;
			return new CardEffect.Result{result = player.resourceRerollCount, type = CardEffect.Type.RerollCountUp};
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
				player.resource[i] = Util.GetRandomEnumValue<ResourceType>();
			}
			return new CardEffect.Result{result = player.resource, type = CardEffect.Type.ForceReroll};
		}

		private static CardEffect.Result CreateToMe(Card card, GamePlayer player, ParameterList parameters)
		{
			int id = parameters[0].Get<int>(player);
			int amount = parameters[1].Get<int>(player);

			Card[] createdCards = new Card[amount];
			for (int i = 0; i < amount; i++)
			{
				createdCards[i] = CardManager.CloneCard(id);
				createdCards[i].death = true;
				player.hand.Add(createdCards[i]);
			}
			return new CardEffect.Result{result = createdCards, type = CardEffect.Type.CreateToMe};
		}
		
		//구현 미완료
		private static CardEffect.Result CreateToOther(Card card, GamePlayer player, ParameterList parameters)
		{
			int id = parameters[0].Get<int>(player);
			int amount = parameters[1].Get<int>(player);
			int type = parameters[2].Get<int>(player);
			int placeOrRange = parameters[3].Get<int>(player);

			Card[] createdCards = new Card[amount];
			for (int i = 0; i < amount; i++)
			{
				createdCards[i] = CardManager.CloneCard(id);
				createdCards[i].death = true;
			}
			
			
			CardEffect.Result _CreateToOther(Card card, GamePlayer player, ParameterList parameters)
			{
				if (type == 0)
				{
					int place = placeOrRange;
					var targetPlayer = player.parent.Find(x => x.rank == place);
					foreach (var c in createdCards)
						targetPlayer?.AddCardToDeck(c);
				}
				else if (type == 1)
				{
					int range = placeOrRange;
					// player.parent.FindAll(x => x.rank >= player.rank-1)
				}
				else
				{

				}
	
				return createdCards;
			}

			CardEffect cardEffect = new CardEffect(new List<CardEffect.Element>())

			player.AddPreheatEndEvent(_CreateToOther);
			return new CardEffect.Result{result = null, type = CardEffect.Type.CreateToMe};
		}	
		
		private static CardEffect.Result BuffToMe(Card card, GamePlayer player, ParameterList parameters)
		{
			// int id = parameters[0].Get<int>(player);
			// int amount = parameters[1].Get<int>(player);
			return null;
		}	
		
		private static CardEffect.Result BuffToOther(Card card, GamePlayer player, ParameterList parameters)
		{
			// int id = parameters[0].Get<int>(player);
			// int amount = parameters[1].Get<int>(player);
			return null;
		}	
		
		private static CardEffect.Result EraseBuff(Card card, GamePlayer player, ParameterList parameters)
		{
			// int id = parameters[0].Get<int>(player);
			// int amount = parameters[1].Get<int>(player);
			return null;
		}	
		
		private static CardEffect.Result Overload(Card card, GamePlayer player, ParameterList parameters)
		{
			int @default = parameters[0].Get<int>(player);
			int amount = parameters[1].Get<int>(player);
			CardEffect effect = parameters[2].Get<CardEffect>(player);

			effect.Use(card, player);
			
			return null;
		}	

		
		private static CardEffect.Result Mount(Card card, GamePlayer player, ParameterList parameters)
		{
			// int id = parameters[0].Get<int>(player);
			// int amount = parameters[1].Get<int>(player);
			return null;
		}	
		
		private static CardEffect.Result Combo(Card card, GamePlayer player, ParameterList parameters)
		{
			List<int> idList = parameters[0].Get<List<int>>(player);
			CardEffect effect = parameters[1].Get<CardEffect>(player);

			bool isComboReady = true;
			foreach (var id in idList)
			{
				Card? find = player.turnUsedCard.Find(x => x.id == id);
				isComboReady = isComboReady && (find != null);
			}

			if (isComboReady)
			{
				effect.Use(card, player);
				effect.GetTypes();
			}
			return null;
		}	
		
		private static CardEffect.Result EnforceSelf(Card card, GamePlayer player, ParameterList parameters)
		{
			// int id = parameters[0].Get<int>(player);
			// int amount = parameters[1].Get<int>(player);
			return null;
		}	
		
		private static CardEffect.Result Discard(Card card, GamePlayer player, ParameterList parameters)
		{
			// int id = parameters[0].Get<int>(player);
			// int amount = parameters[1].Get<int>(player);
			return null;
		}	
		
		private static CardEffect.Result Choice(Card card, GamePlayer player, ParameterList parameters)
		{
			// int id = parameters[0].Get<int>(player);
			// int amount = parameters[1].Get<int>(player);
			return null;
		}	
	}
}