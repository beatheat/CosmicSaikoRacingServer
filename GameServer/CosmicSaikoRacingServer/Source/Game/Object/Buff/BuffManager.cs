using System.Text.Json.Serialization;

namespace CSRServer.Game
{
	public class BuffManager
	{
		public List<Buff> buffList;
		
		public BuffManager(GamePlayer owner)
		{
			buffList = new List<Buff>
			{
				//버프
				new BuffElectricLeak(owner),
				new BuffProliferation(owner),
				new BuffExposure(owner),
				new BuffBreakDown(owner),
				//디버프
				new BuffHighEfficiency(owner),
				new BuffRefine(owner),
				new BuffHighDensity(owner),
				new BuffMimesis(owner)
			};
		}

		public void AddBuff(Buff.Type buff, int count)
		{
			int buffIndex = (int) buff;
			if (buffIndex < 0 || buffIndex >= buffList.Count)
				return;
			buffList[(int) buff].Add(count);
		}

		public int Release(Buff.Type buff)
		{
			int buffIndex = (int) buff;
			if (buffIndex < 0 || buffIndex >= buffList.Count)
				return 0;
			return buffList[(int) buff].Release();
		}
		
		public int ReleaseAll()
		{
			int amount = 0;
			foreach (var buff in buffList)
			{
				amount += buff.Release();
			}
			return amount;
		}
		
		public int GetBuffCount(Buff.Type buff)
		{
			int buffIndex = (int) buff;
			if (buffIndex < 0 || buffIndex >= buffList.Count)
				return 0;
			return buffList[buffIndex].count;
		}

		public void OnTurnStart()
		{
			foreach (var buff in buffList)
			{
				buff.OnTurnStart();
			}
		}
		
		//false반환시 카드 사용 불가
		public bool BeforeUseCard(ref Card card, ref CardEffectModule.Result[] results)
		{
			bool check = true;
			foreach (var buff in buffList)
			{
				check = check && buff.BeforeUseCard(ref card, ref results);
			}
			return check;
		}
		
		public void AfterUseCard(ref Card card)
		{
			foreach (var buff in buffList)
			{
				buff.AfterUseCard(ref card);
			}
		}

		public void BeforeRerollResource(ref List<int>? resourceFixed)
		{
			foreach (var buff in buffList)
			{
				buff.BeforeRerollResource(ref resourceFixed);
			}
		}

		public void AfterRerollResource(ref List<int>? resourceFixed, ref List<Resource.Type> resourceReel)
		{
			foreach (var buff in buffList)
			{
				buff.AfterRerollResource(ref resourceFixed, ref resourceReel);
			}
		}

		public void OnDrawCard(ref Card card)
		{
			foreach (var buff in buffList)
			{
				buff.OnDrawCard(ref card);
			}
		}

		public void OnThrowCard(Card card)
		{
			foreach (var buff in buffList)
			{
				buff.OnThrowCard(card);
			}
		}

		public void OnTurnEnd()
		{
			foreach (var buff in buffList)
			{
				buff.OnTurnEnd();
			}
		}
	}
}