using CSR.Game.Phase;
using ProtoBuf;

namespace CSR.Game.GameObject;

[ProtoContract]
[ProtoInclude(10, typeof(BuffBreakDown))]
[ProtoInclude(11, typeof(BuffElectricLeak))]
[ProtoInclude(12, typeof(BuffExposure))]
[ProtoInclude(13, typeof(BuffHighDensity))]
[ProtoInclude(14, typeof(BuffHighEfficiency))]
[ProtoInclude(15, typeof(BuffMimesis))]
[ProtoInclude(16, typeof(BuffProliferation))]
[ProtoInclude(17, typeof(BuffRefine))]
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

	public virtual bool BeforeUseCard(Card card, ref CardEffect.Result result) { return true;}
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