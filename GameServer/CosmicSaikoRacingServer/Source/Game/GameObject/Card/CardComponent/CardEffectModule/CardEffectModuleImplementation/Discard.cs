using CSR.Game.Phase;
using ProtoBuf;

#pragma warning disable CS8618

namespace CSR.Game.GameObject.CardEffectModuleImplementation;


public class Discard : CardEffectModule
{
	[ProtoContract]
	public class ResultDiscard : Result
	{
		[ProtoMember(2)]
		public List<CardEffect.Result> LeakResults { get; set; }
		[ProtoMember(3)]
		public List<CardEffect.Result> DiscardResults { get; set; }
		[ProtoMember(4)]
		public List<int> ThrowIndexList { get; set; }
	}

	public Discard(List<Parameter> parameters) : base(parameters, Type.Discard)
	{
	}

	public override Result Activate(PreheatPhase preheatPhase, Card card, GamePlayer player)
	{
		int amount = parameters.Get<int>(0, card, player);
		CardEffect effect = parameters.Get<CardEffect>(1, card, player) ?? CardEffect.Nothing();
		Random random = new Random();

		if (amount > player.Card.Hand.Count)
			amount = player.Card.Hand.Count;

		var leakResults = new List<CardEffect.Result>();
		var discardResults = new List<CardEffect.Result>();
		List<int> throwIndexList = new List<int>();
		//amount장의 카드를 버리고 leak효과+ discard특수효과를 발동한다.
		for (int i = 0; i < amount; i++)
		{
			int throwIndex = random.Next(player.Card.Hand.Count);
			throwIndexList.Add(throwIndex);
			var throwResult = player.ThrowCard(throwIndex, preheatPhase);
			leakResults.Add(throwResult);
			var discardResult = effect.Use(preheatPhase, card, player);
			discardResults.Add(discardResult);
		}

		var result = new Dictionary<string, object> {["throwIndexList"] = throwIndexList, ["leakResults"] = leakResults, ["discardResults"] = discardResults};

		return new ResultDiscard {LeakResults = leakResults, DiscardResults = discardResults, ThrowIndexList = throwIndexList, Type = Type.Discard};	
	}
}
