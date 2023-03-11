using System.Reflection.Metadata;
using System.Text.Json.Serialization;

namespace CSRServer.Game
{
	public class Obstacle
	{
		public enum Type
		{
			Card,
			Buff
		}

		public class Result
		{
			public List<int> activatePlayers = new List<int>();
			public Type type;
			public int amount;
			public object? result;
		}

		public readonly Type type;
		public readonly int id;
		public readonly int amount;
		public readonly int location;

		[JsonIgnore]
		private readonly bool isDeath;
		[JsonIgnore]
		private readonly List<GamePlayer> activatePlayerList;


		public Obstacle(Type type, int location, int id, int amount, bool isDeath = false)
		{
			this.location = location;
			this.type = type;
			this.id = id;
			this.amount = amount;
			this.isDeath = isDeath;
			this.activatePlayerList = new List<GamePlayer>();
		}

		public void SetActivatePlayer(GamePlayer player)
		{
			activatePlayerList.Add(player);
		}

		public bool Activate(out Result result)
		{
			result = new Result();
			if (activatePlayerList.Count == 0)
				return false;

			Card[] createdCards = Array.Empty<Card>();
			if (type == Type.Card)
			{
				createdCards = new Card[amount];
				for (int i = 0; i < amount; i++)
				{
					createdCards[i] = CardManager.GetCard(id);
					if (isDeath)
						createdCards[i].death = true;
				}

				result.result = createdCards;
				result.amount = amount;
				result.type = Type.Card;
			}
			else
			{
				result.result = (Buff.Type) id;
				result.amount = amount;
				result.type = Type.Buff;
			}

			foreach (var player in activatePlayerList)
			{
				if (type == Type.Card)
					player.AddCardToDeck(createdCards);
				else
					player.AddBuff((Buff.Type) id, amount);
				result.activatePlayers.Add(player.index);
			}

			return true;
		}
	}
}