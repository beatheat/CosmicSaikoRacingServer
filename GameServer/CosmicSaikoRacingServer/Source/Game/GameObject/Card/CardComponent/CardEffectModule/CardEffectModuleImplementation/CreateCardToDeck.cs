using CSR.Game.Phase;
using ProtoBuf;

#pragma warning disable CS8618

namespace CSR.Game.GameObject.CardEffectModuleImplementation;

public class CreateCardToDeck : CardEffectModule
{
	[ProtoContract]
	public class ResultCreateCardToDeck : Result
	{
		[ProtoMember(2)]
		public List<Card> CreatedCards { get; set; }
	}

	public CreateCardToDeck(List<Parameter> parameters) : base(parameters, Type.CreateCardToDeck)
	{
	}

	public override Result Activate(PreheatPhase preheatPhase, Card card, GamePlayer player)
	{
		int id = parameters.Get<int>(0, card, player);
		int amount = parameters.Get<int>(1, card, player);
		bool isDeath = parameters.Get<bool>(2, card, player);

		Card[] createdCards = new Card[amount];
		for (int i = 0; i < amount; i++)
		{
			createdCards[i] = CardManager.GetCard(id);
			if (isDeath)
				createdCards[i].Death = true;
			player.AddCardToDeck(createdCards[i]);
		}

		return new ResultCreateCardToDeck {CreatedCards = createdCards.ToList(), Type = Type.CreateCardToDeck};	
	}
}
