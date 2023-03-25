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

		#region Buff Activation Methods

		public virtual void OnPreheatStart() { }
		public virtual bool BeforeUseCard(ref Card card, ref CardEffectModule.Result[] results) { return true; }
		public virtual void AfterUseCard(ref Card card) { }

		public virtual void BeforeRerollResource(ref List<int>? resourceFixed) { }
		public virtual void AfterRerollResource(ref List<int>? resourceFixed, ref List<Resource.Type> resourceReel) { }
		public virtual void OnThrowCard(Card card) { }
		public virtual void OnDrawCard(ref Card card) { }
		
		public virtual void OnDepartStart() { }

		public virtual int Release()
		{
			int releaseCount = count;
			count = 0;
			return releaseCount;
		}

		public virtual void OnPreheatEnd()
		{
			Release();
		}
		
		#endregion
	}
}