namespace CSRServer.Game
{
	using ParameterList = List<CardEffect.Parameter>;
	using EffectModule = Func<Card, GamePlayer, List<CardEffect.Parameter>, object>;
	
	internal class CardEffectModule
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
		private static object Nothing(Card card, GamePlayer player, ParameterList parameters)
		{
			return null!;
		}

		private static object Add(Card card, GamePlayer player, ParameterList parameters)
		{
			int amount = parameters[0].Get<int>(player);
			player.currentDistance += amount;
			return player.currentDistance;
		}
		
		private static object Multiply(Card card, GamePlayer player, ParameterList parameters)
		{
			double amount = parameters[0].Get<double>(player);
			player.currentDistance = (int)(player.currentDistance * amount);
			return player.currentDistance;
		}

		private static object Draw(Card card, GamePlayer player, ParameterList parameters)
		{
			int amount = parameters[0].Get<int>(player);
			return player.DrawCard(amount);
		}
		
		private static object RerollCountUp(Card card, GamePlayer player, ParameterList parameters)
		{
			int amount = parameters[0].Get<int>(player);
			return ++player.resourceRerollCount;
		}
		
		private static object Death(Card card, GamePlayer player, ParameterList parameters)
		{
			return (card.death = true);
		}

		private static object Fail(Card card, GamePlayer player, ParameterList parameters)
		{
			return (card.enable = false);
		}
	}
}