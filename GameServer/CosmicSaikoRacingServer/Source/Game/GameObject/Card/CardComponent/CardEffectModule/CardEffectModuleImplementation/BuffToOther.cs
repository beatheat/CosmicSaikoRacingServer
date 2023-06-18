using CSR.Game.Phase;
using ProtoBuf;

#pragma warning disable CS8618

namespace CSR.Game.GameObject.CardEffectModuleImplementation;


public class BuffToOther : CardEffectModule
{
	[ProtoContract]
	public class ResultBuffToOther : Result
	{
		[ProtoMember(2)]
		public BuffType BuffType { get; set; }
		[ProtoMember(3)]
		public int BuffCount { get; set; }
		[ProtoMember(4)]
		public int SourceIndex { get; set; }
		[ProtoMember(5)]
		public List<int> DestinationIndexList { get; set; }
	}

	public BuffToOther(List<Parameter> parameters) : base(parameters, Type.BuffToOther)
	{
	}

	public override Result Activate(PreheatPhase preheatPhase, Card card, GamePlayer player)
	{
		int id = parameters.Get<int>(0, card, player);
		int amount = parameters.Get<int>(1, card, player);
		int target = parameters.Get<int>(2, card, player);


		List<int> targetPlayerIndex = new List<int>();
		//자신을 제외한 모든 플레이어
		if (target == 0)
		{
			foreach (var p in player.parent)
			{
				if (p != player)
					targetPlayerIndex.Add(p.Index);
			}
		}
		// 해당 등수
		else if (target is >= 1 and <= 4)
		{
			var p = player.parent.Find(x => x.Rank == target);
			if (p != null && p != player)
				targetPlayerIndex.Add(p!.Index);
		}
		//자신 바로 앞 플레이어
		else if (target == 5)
		{
			if (player.Rank > 1)
			{
				var p = player.parent.Find(x => x.Rank == player.Rank - 1);
				if (p != null)
					targetPlayerIndex.Add(p.Index);
			}
		}
		//자신 바로 뒤 플레이어
		else if (target == 6)
		{
			if (player.Rank < 4)
			{
				var p = player.parent.Find(x => x.Rank == player.Rank + 1);
				if (p != null)
					targetPlayerIndex.Add(p.Index);
			}
		}
		// target->distance 범위 안의 플레이어
		else if (target >= 7)
		{
			int distance = target;
			foreach (var p in player.parent)
			{
				if (p != player && Math.Abs(p.CurrentDistance - player.CurrentDistance) <= distance)
					targetPlayerIndex.Add(p.Index);
			}
		}


		var result = new ResultBuffToOther
		{
			BuffType = (BuffType) id,
			BuffCount = amount,
			SourceIndex = player.Index,
			DestinationIndexList = targetPlayerIndex,
			Type = Type.BuffToOther
		};
		
		Result _BuffToOther()
		{
			foreach (int idx in targetPlayerIndex)
			{
				player.parent[idx].AddBuff((BuffType) id, amount);
			}
			return result;
		}

		if (targetPlayerIndex.Count == 0)
		{
			return new Result {Type = Type.Nothing};
		}

		player.AddDepartEvent(_BuffToOther);
		return result;
	}
}
