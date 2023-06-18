using CSR.Game.Phase;
using ProtoBuf;

#pragma warning disable CS8618

namespace CSR.Game.GameObject.CardEffectModuleImplementation;

public class ForceReroll : CardEffectModule
{
	[ProtoContract]
	public class ResultForceReroll : Result
	{
		[ProtoMember(2)]
		public List<ResourceType> ResourceReel { get; set; }
	}

	public ForceReroll(List<Parameter> parameters) : base(parameters, Type.ForceReroll)
	{
	}

	public override Result Activate(PreheatPhase preheatPhase, Card card, GamePlayer player)
	{
		for (int i = 0; i < player.Resource.ReelCount; i++)
		{
			player.Resource.Reel[i] = Util.GetRandomEnumValue<ResourceType>();
		}

		return new ResultForceReroll {ResourceReel = player.Resource.Reel, Type = Type.ForceReroll};	
	}
}
