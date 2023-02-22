using System.Reflection.Metadata;
using System.Text.Json.Serialization;

namespace CSRServer.Game.Mount;

internal class Obstacle
{
	public enum Type
	{
		CARD, BUFF
	}

	private readonly Type type;
	private readonly int id;
	private readonly int amount;
	[JsonIgnore]
	private readonly bool isDeath;
	
	public bool isActivated;
	
	public int location;

	public Obstacle(Type type, int location, int id, int amount, bool isDeath = false)
	{
		this.location = location;
		this.type = type;
		this.id = id;
		this.amount = amount;
		this.isDeath = isDeath;
		this.isActivated = false;
	}

	public object Activate(GamePlayer player)
	{
		isActivated = true;
		if (type == Type.CARD)
		{
			Card[] createdCards = new Card[amount];
			for (int i = 0; i < amount; i++)
			{
				createdCards[i] = CardManager.GetCard(id);
				if(isDeath)
					createdCards[i].death = true;
				player.AddCardToDeck(createdCards[i]);
				
			}
			return createdCards;
		}
		else
		{
			player.AddBuff((Buff.Type) id, amount);
			return (Buff.Type) id;
		}

	}
}