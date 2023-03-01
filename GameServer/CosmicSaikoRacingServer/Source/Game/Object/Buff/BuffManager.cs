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
			Add(buffs, Buff.Type.LowEfficiency, LowEfficiency);
			return buffs;
		}

		private static void Add(Dictionary<Buff.Type, Buff> buffs, Buff.Type type, BuffEffect effect)
		{
			buffs.Add(type, new Buff(type, effect));
		}

		private static void ElectricLeak(Buff buff, GamePlayer player)
		{
			if (!buff.variables.ContainsKey("resourceLockIndexList"))
				buff.variables.Add("resourceLockIndexList", new List<int>());
			Random random = new Random();
			int lockCount = buff.count > player.resourceCount ?  player.resourceCount : buff.count;
			List<int> lockIndex = new List<int>(lockCount);
			for (int i = 0; i < lockCount; i++)
			{
				int randomIndex;
				do
				{
					randomIndex = random.Next(player.resourceCount);
					lockIndex.Add(randomIndex);
				} while (lockIndex.FindIndex(x => x == randomIndex) > 0);
			}

			buff.variables["resourceLockIndexList"] = lockIndex;
		}
		
		private static void Proliferation(Buff buff, GamePlayer player)
		{
			if (!buff.variables.ContainsKey("resourceCondition"))
				buff.variables.Add("resourceCondition", new List<int>());

			foreach (var resource in player.resourceReel)
			{
				//걍랜덤
			}
			
		}	
		private static void Exposure(Buff buff, GamePlayer player)
		{
			if (!buff.variables.ContainsKey("cardExposureIndexList"))
				buff.variables.Add("cardExposureIndexList", new List<int>());
			
		}	
		private static void BreakDown(Buff buff, GamePlayer player)
		{

		}	
		private static void HighEfficiency(Buff buff, GamePlayer player)
		{

		}	
		private static void LowEfficiency(Buff buff, GamePlayer player)
		{

		}	
	}
}