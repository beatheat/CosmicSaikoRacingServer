﻿using System.Text.Json.Serialization;

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
		public bool BeforeUseCard(ref Card card)
		{
			bool check = true;
			foreach (var buff in buffList)
			{
				check = check && buff.BeforeUseCard(ref card);
			}
			return check;
		}
		
		public void AfterUseCard(ref Card card, ref CardEffect.Result[] results)
		{
			foreach (var buff in buffList)
			{
				buff.AfterUseCard(ref card,ref results);
			}
		}

		public void BeforeRollResource(ref List<int>? resourceFixed)
		{
			foreach (var buff in buffList)
			{
				buff.BeforeRollResource(ref resourceFixed);
			}
		}

		public void AfterRollResource(ref List<int>? resourceFixed, ref List<Resource.Type> resourceReel)
		{
			foreach (var buff in buffList)
			{
				buff.AfterRollResource(ref resourceFixed, ref resourceReel);
			}
		}
		

		public void OnDrawCard(ref Card card)
		{
			foreach (var buff in buffList)
			{
				buff.OnDrawCard(ref card);
			}
		}

		public void OnDiscardCard(Card card)
		{
			foreach (var buff in buffList)
			{
				buff.OnDiscardCard(card);
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