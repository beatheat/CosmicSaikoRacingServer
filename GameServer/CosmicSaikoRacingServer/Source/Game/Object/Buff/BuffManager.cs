namespace CSRServer.Game
{
	delegate void BuffEffect(Buff buff, GamePlayer player);
	internal static class BuffManager
	{
		public static Dictionary<Buff.Type, Buff> CreateBuffDictionary()
		{
			Dictionary<Buff.Type, Buff> buffs = new Dictionary<Buff.Type, Buff>();
			Add(buffs, Buff.Type.ElectricLeak, ElectricLeak);
			Add(buffs, Buff.Type.Proliferation, Proliferation);
			Add(buffs, Buff.Type.Exposure, Exposure);
			Add(buffs, Buff.Type.BreakDown, BreakDown);
			Add(buffs, Buff.Type.HighEfficiency, HighEfficiency);
			Add(buffs, Buff.Type.Immune, Immune);
			Add(buffs, Buff.Type.HighDensity, HighDensity);
			Add(buffs, Buff.Type.Mimesis, Mimesis);

			return buffs;
		}

		private static void Add(Dictionary<Buff.Type, Buff> buffs, Buff.Type type, BuffEffect effect)
		{
			buffs.Add(type, new Buff(type, effect));
		}

		private static void ElectricLeak(Buff buff, GamePlayer player)
		{
			if (buff.count == 0) return;
			if (!buff.variables.ContainsKey("resourceLockIndexList"))
				buff.variables.Add("resourceLockIndexList", new List<int>());
			Random random = new Random();
			int lockCount = buff.count > player.resourceReelCount ?  player.resourceReelCount : buff.count;
			List<int> lockIndex = new List<int>(lockCount);
			for (int i = 0; i < lockCount; i++)
			{
				int randomIndex;
				do
				{
					randomIndex = random.Next(player.resourceReelCount);
					lockIndex.Add(randomIndex);
				} while (lockIndex.FindIndex(x => x == randomIndex) > 0);
			}

			buff.variables["resourceLockIndexList"] = lockIndex;
		}
		
		private static void Proliferation(Buff buff, GamePlayer player)
		{
			if (buff.count == 0) return;
			if (!buff.variables.ContainsKey("resourceCondition"))
				buff.variables.Add("resourceCondition", new List<Resource.Type>());

			int count = buff.count switch
			{
				>= 1 and <= 3 => 2,
				>= 4 and <= 6 => 3,
				>= 7 and <= 9 => 4,
				>= 10 => 5,
				_ => 0
			};
			List<Resource.Type> lockCondition = new List<Resource.Type>(count);
			for (int i = 0; i < count; i++)
				lockCondition[i] = Util.GetRandomEnumValue<Resource.Type>();
			buff.variables["resourceCondition"] = lockCondition;
		}	
		
		private static void Exposure(Buff buff, GamePlayer player)
		{
			if (buff.count == 0) return;
			
			var cardConditions = new List<CardCondition>();
			var random = new Random();

			//피폭되지 않은 카드만 고른다
			var nonExposureHand = player.hand.FindAll(x => x.isExposure == false);
			int count = buff.count;
			if (count > nonExposureHand.Count)
			{
				count = nonExposureHand.Count;
				buff.count -= nonExposureHand.Count;
			}
			//랜덤 조건 생성
			for (int i = 0; i < count; i++)
			{
				//same
				if (random.Next(2) == 0)
				{
					int randomNumber = random.Next(2,7);
					if (randomNumber == 6)
					{
						cardConditions.Add(new CardCondition(22));
					}
					else
					{
						cardConditions.Add(new CardCondition(randomNumber));
					}
				}
				else
				{
					int randomNumber = random.Next(1, 6);
					var lockCondition = new List<Resource.Type>(randomNumber);
					for (int j = 0; j < randomNumber; j++)
						lockCondition[j] = Util.GetRandomEnumValue<Resource.Type>();
					cardConditions.Add(new CardCondition(lockCondition));
				}
			}
			//랜덤 패에 부여
			Util.DistributeOnList(nonExposureHand, count, out var selectedCards);
			foreach (var card in selectedCards)
			{
				card.isExposure = true;
				card._condition = cardConditions[0];
				card.condition = card._condition;
				cardConditions.RemoveAt(0);
			}

		}	
		
		private static void BreakDown(Buff buff, GamePlayer player) { }	
		private static void HighEfficiency(Buff buff, GamePlayer player) { }	
		private static void Immune(Buff buff, GamePlayer player) { }	
		private static void HighDensity(Buff buff, GamePlayer player) { }

		private static void Mimesis(Buff buff, GamePlayer player)
		{
			if (buff.count == 0) return;
			
			var random = new Random();

			//의태하지 않은 카드만 고른다
			var nonMimesisHand = player.hand.FindAll(x => x.isMimesis == false);
			int count = buff.count;
			if (count > nonMimesisHand.Count)
			{
				count = nonMimesisHand.Count;
				buff.count -= nonMimesisHand.Count;
			}
	
			//랜덤 패에 부여
			Util.DistributeOnList(nonMimesisHand, count, out var selectedCards);
			foreach (var card in selectedCards)
			{
				card.isMimesis = true;
				card._condition = new CardCondition(0);
				card.condition = card._condition;
			}

		}	
	}
}