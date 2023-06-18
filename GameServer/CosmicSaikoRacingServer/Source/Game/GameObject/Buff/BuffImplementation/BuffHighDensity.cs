using CSR.Game.Phase;
using ProtoBuf;

namespace CSR.Game.GameObject;

[ProtoContract]
internal class BuffHighDensity : Buff
{
	public BuffHighDensity(PreheatPhase phase, GamePlayer player) : base(phase, player)
	{
		Type = BuffType.HighDensity;
	}

	/// <summary>
	/// 고밀도 버프가 있을 경우 카드가 두번 발동함
	/// </summary>
	public override bool BeforeUseCard(Card card, ref CardEffect.Result result)
	{
		if (Count > 0)
		{
			result.Concat(card.UseEffect(phase, player));
			Count--;
		}

		return true;
	}
}