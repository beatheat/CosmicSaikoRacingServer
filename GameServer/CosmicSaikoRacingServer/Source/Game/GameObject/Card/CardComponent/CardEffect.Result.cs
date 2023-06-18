using ProtoBuf;

namespace CSR.Game.GameObject;

public partial class CardEffect
{
	[ProtoContract]
	public class Result
	{
		[ProtoMember(1)]
		public List<CardEffectModule.Result> ModuleResults { get; set; }

		public Result()
		{
			ModuleResults = new List<CardEffectModule.Result>();
		}

		public Result(CardEffectModule.Result result)
		{
			ModuleResults = new List<CardEffectModule.Result> {result};
		}
		
		public Result(List<CardEffectModule.Result> results)
		{
			ModuleResults = results;
		}

		public void Concat(Result other)
		{
			ModuleResults.AddRange(other.ModuleResults);
		}
	}
}