using CSR.Game.GameObject;
using CSR.Game.Phase;

namespace CSR.Game.Player;

public static class BuffLogic
{
	public static void InitBuff(this GamePlayer player, PreheatPhase phase)
	{
		player.Buff.List = new List<Buff>
		{
			//버프
			new BuffElectricLeak(phase, player),
			new BuffProliferation(phase, player),
			new BuffExposure(phase, player),
			new BuffBreakDown(phase, player),
			//디버프
			new BuffHighEfficiency(phase, player),
			new BuffRefine(phase, player),
			new BuffHighDensity(phase, player),
			new BuffMimesis(phase, player)
		};
	}
	
	public static void AddBuff(this GamePlayer player, BuffType buff, int count)
	{
		int buffIndex = (int) buff;
		if (buffIndex < 0 || buffIndex >= player.Buff.List.Count)
			return;
		player.Buff.List[(int) buff].Add(count);
	}

	public static int ReleaseBuff(this GamePlayer player, BuffType buff)
	{
		int buffIndex = (int) buff;
		if (buffIndex < 0 || buffIndex >= player.Buff.List.Count)
			return 0;
		return player.Buff.List[(int) buff].Release();
	}

	public static int ReleaseBuffAll(this GamePlayer player)
	{
		int amount = 0;
		foreach (var buff in player.Buff.List)
		{
			amount += buff.Release();
		}

		return amount;
	}

	public static int GetBuffCount(this GamePlayer player, BuffType buff)
	{
		int buffIndex = (int) buff;
		if (buffIndex < 0 || buffIndex >= player.Buff.List.Count)
			return 0;
		return player.Buff.List[buffIndex].Count;
	}

	public static void BuffOnPreheatStart(this GamePlayer player)
	{
		foreach (var buff in player.Buff.List)
		{
			buff.OnPreheatStart();
		}
	}

	//false반환시 카드 사용 불가
	public static bool BuffBeforeUseCard(this GamePlayer player, Card card, ref CardEffectModule.Result[] results)
	{
		bool check = true;
		foreach (var buff in player.Buff.List)
		{
			check = check && buff.BeforeUseCard(card, ref results);
		}

		return check;
	}

	public static void BuffAfterUseCard(this GamePlayer player, Card card)
	{
		foreach (var buff in player.Buff.List)
		{
			buff.AfterUseCard(card);
		}
	}

	public static void BuffBeforeRerollResource(this GamePlayer player, ref List<int>? resourceFixed)
	{
		foreach (var buff in player.Buff.List)
		{
			buff.BeforeRerollResource(ref resourceFixed);
		}
	}

	public static void BuffAfterRerollResource(this GamePlayer player, List<int>? resourceFixed)
	{
		foreach (var buff in player.Buff.List)
		{
			buff.AfterRerollResource(resourceFixed);
		}
	}

	public static void BuffOnDrawCard(this GamePlayer player, Card card)
	{
		foreach (var buff in player.Buff.List)
		{
			buff.OnDrawCard(card);
		}
	}

	public static void BuffOnThrowCard(this GamePlayer player, Card card)
	{
		foreach (var buff in player.Buff.List)
		{
			buff.OnThrowCard(card);
		}
	}

	public static void BuffOnPreheatEnd(this GamePlayer player)
	{
		foreach (var buff in player.Buff.List)
		{
			buff.OnPreheatEnd();
		}
	}

	public static void BuffOnDepartStart(this GamePlayer player)
	{
		foreach (var buff in player.Buff.List)
		{
			buff.OnDepartStart();
		}
	}
}