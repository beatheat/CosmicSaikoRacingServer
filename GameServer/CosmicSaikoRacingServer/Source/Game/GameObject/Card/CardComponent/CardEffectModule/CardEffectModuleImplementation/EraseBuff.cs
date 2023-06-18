using CSR.Game.Phase;
using ProtoBuf;

#pragma warning disable CS8618


namespace CSR.Game.GameObject.CardEffectModuleImplementation;

public class EraseBuff : CardEffectModule
{
	[ProtoContract]
	public class ResultEraseBuff : Result
	{
		[ProtoMember(2)]
		public List<CardEffect.Result> Results { get; set; }
	}

	public EraseBuff(List<Parameter> parameters) : base(parameters, Type.EraseBuff)
	{
	}

	public override Result Activate(PreheatPhase preheatPhase, Card card, GamePlayer player)
	{
		int id = parameters.Get<int>(0, card, player);
		CardEffect effect = parameters.Get<CardEffect>(1, card, player) ?? CardEffect.Nothing();
		var results = new List<CardEffect.Result>();
		//모든 버프 제거
		if (id == 99)
		{
			int amount = player.ReleaseBuffAll();
			for (int i = 0; i < amount; i++)
				results.Add(effect.Use(preheatPhase, card, player));
		}
		//특정버프제거
		else
		{
			int amount = player.ReleaseBuff((BuffType) id);
			for (int i = 0; i < amount; i++)
				results.Add(effect.Use(preheatPhase, card, player));

		}

		return new ResultEraseBuff { Results = results, Type = Type.EraseBuff};	
	}
}
