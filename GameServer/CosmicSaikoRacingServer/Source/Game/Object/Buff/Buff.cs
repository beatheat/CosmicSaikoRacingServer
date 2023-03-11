using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSRServer.Game
{
	[JsonDerivedType(typeof(Buff))]
	[JsonDerivedType(typeof(BuffBreakDown))]
	[JsonDerivedType(typeof(BuffElectricLeak))]
	[JsonDerivedType(typeof(BuffExposure))]
	[JsonDerivedType(typeof(BuffHighDensity))]
	[JsonDerivedType(typeof(BuffHighEfficiency))]
	[JsonDerivedType(typeof(BuffMimesis))]
	[JsonDerivedType(typeof(BuffProliferation))]
	[JsonDerivedType(typeof(BuffRefine))]
	public class Buff
	{
		public enum Type
		{
			ElectricLeak, Proliferation, Exposure, BreakDown, HighEfficiency, Refine, HighDensity, Mimesis
		}
		
		public Type type { protected set; get; }
		public int count { protected set; get; }

		[JsonIgnore]
		protected GamePlayer player;
		
		protected Buff(GamePlayer player)
		{
			this.count = 0;
			this.player = player;
		}

		public void Add(int count)
		{
			this.count += count;
		}
		
		public virtual void OnTurnStart() { }
		public virtual bool BeforeUseCard(ref Card card) { return true; }
		public virtual void AfterUseCard(ref Card card, ref CardEffect.Result[] results) { }

		public virtual void OnRollResource(ref List<int>? resourceFixed) { }
		public virtual void OnDiscardCard(Card card) { }
		public virtual void OnDrawCard(ref Card card) { }

		public virtual int Release()
		{
			int releaseCount = count;
			count = 0;
			return releaseCount;
		}

		public virtual void OnTurnEnd()
		{
			Release();
		}
	}
}