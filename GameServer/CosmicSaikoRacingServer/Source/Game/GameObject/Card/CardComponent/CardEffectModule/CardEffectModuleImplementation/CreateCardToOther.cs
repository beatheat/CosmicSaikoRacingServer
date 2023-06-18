using CSR.Game.Phase;
using ProtoBuf;

#pragma warning disable CS8618

namespace CSR.Game.GameObject.CardEffectModuleImplementation;


public class CreateCardToOther : CardEffectModule
{
	[ProtoContract]
	public class ResultCreateCardToOther : Result
	{
		[ProtoMember(2)]
		public List<Card> CreatedCards { get; set; }
		
		[ProtoMember(3)]
		public int SourceIndex { get; set; }
		
		[ProtoMember(4)]
		public List<int> DestinationIndexList { get; set; }
	}

	public CreateCardToOther(List<Parameter> parameters) : base(parameters, Type.CreateCardToOther)
	{
	}

	public override Result Activate(PreheatPhase preheatPhase, Card card, GamePlayer player)
	{
		int id = parameters.Get<int>(0, card, player);
		int amount = parameters.Get<int>(1, card, player);
		int target = parameters.Get<int>(2, card, player);
		bool isDeath = parameters.Get<bool>(3, card, player);


		Card[] createdCards = new Card[amount];
		for (int i = 0; i < amount; i++)
		{
			createdCards[i] = CardManager.GetCard(id);
			if (isDeath)
				createdCards[i].Death = true;
		}

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
			if (p != player && p != null)
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
		
		var result = new ResultCreateCardToOther
		{
			CreatedCards = createdCards.ToList(), 
			SourceIndex = player.Index, 
			DestinationIndexList = targetPlayerIndex, 
			Type = Type.CreateCardToOther
		};
		
		Result CreateToOther()
		{
			foreach (int idx in targetPlayerIndex)
			{
				player.parent[idx].AddCardToDeck(createdCards);
			}
			return result;
		}

		if (targetPlayerIndex.Count == 0)
			return new Result {Type = Type.Nothing};

		player.AddDepartEvent(CreateToOther);
		return result;	
	}
}
