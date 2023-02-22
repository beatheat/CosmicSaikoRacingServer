using System.Reflection.Metadata;
using System.Text.Json.Serialization;

namespace CSRServer.Game
{
	internal class Obstacle
	{
		public enum Type
		{
			CARD,
			BUFF
		}

		public class Result
		{
			public List<int> activatePlayers = new List<int>();
			public Type type;
			public int amount;
			public object? result;
		}

		private readonly Type type;
		private readonly int id;
		private readonly int amount;
		public int location;

		[JsonIgnore]
		private readonly bool isDeath;

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
			if (type == Type.CARD)
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
				result.type = Type.CARD;
			}
			else
			{
				result.result = (Buff.Type) id;
				result.amount = amount;
				result.type = Type.BUFF;
			}

			foreach (var player in activatePlayerList)
			{
				if (type == Type.CARD)
					player.AddCardToDeck(createdCards);
				else
					player.AddBuff((Buff.Type) id, amount);
				result.activatePlayers.Add(player.index);
			}

			return true;
		}
	}
}