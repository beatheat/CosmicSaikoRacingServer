using CSR.Game.Phase;
using CSR.Game.Player;
using ProtoBuf;

namespace CSR.Game.GameObject;

[ProtoContract]
[ProtoInclude(1, typeof(BuffBreakDown))]
[ProtoInclude(2, typeof(BuffElectricLeak))]
[ProtoInclude(3, typeof(BuffExposure))]
[ProtoInclude(4, typeof(BuffHighDensity))]
[ProtoInclude(5, typeof(BuffHighEfficiency))]
[ProtoInclude(6, typeof(BuffMimesis))]
[ProtoInclude(7, typeof(BuffProliferation))]
[ProtoInclude(8, typeof(BuffRefine))]
public class Buff
{

	[ProtoMember(1)]
	public BuffType Type { protected set; get; }
	[ProtoMember(2)]
	public int Count { protected set; get; }

	protected PreheatPhase phase;
	protected GamePlayer player;

	protected Buff(PreheatPhase phase, GamePlayer player)
	{
		this.Count = 0;
		this.player = player;
		this.phase = phase;
	}

	public void Add(int count)
	{
		this.Count += count;
	}

	#region Buff Activation Methods

	public virtual void OnPreheatStart() { }
	public virtual void OnPreheatEnd() { Release(); }

	public virtual bool BeforeUseCard(Card card, ref CardEffectModule.Result[] results) { return true;}
	public virtual void AfterUseCard(Card card){}
	public virtual void BeforeRerollResource(ref List<int>? resourceFixed){}
	public virtual void AfterRerollResource(List<int>? resourceFixed){ }
	public virtual void OnThrowCard(Card card){}
	public virtual void OnDrawCard(Card card){}
	
	public virtual void OnDepartStart(){}

	
	public virtual int Release()
	{
		int releaseCount = Count;
		Count = 0;
		return releaseCount;
	}


	#endregion
}