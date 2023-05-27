using CSR.Game.Phase;
using CSR.Game.Player;
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
	public override bool BeforeUseCard(Card card, ref CardEffectModule.Result[] results)
	{
		if (Count > 0)
		{
			results = results.Concat(card.UseEffect(phase, player)).ToArray();
			Count--;
		}

		return true;
	}
}