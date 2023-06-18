using CSR.Game.Phase;
using ProtoBuf;

namespace CSR.Game.GameObject;

[ProtoContract]
internal class BuffBreakDown : Buff
{
	public BuffBreakDown(PreheatPhase phase, GamePlayer player) : base(phase, player)
	{
		Type = BuffType.BreakDown;
	}

	/// <summary>
	/// 고장 버프: 카드 사용하기 전 50%확률로 카드가 불발한다
	/// </summary>
	public override bool BeforeUseCard(Card card, ref CardEffect.Result result)
	{
		//고장(BREAK_DOWN)버프
		if (Count > 0)
		{
			Random random = new Random();
			if (random.Next(2) != 0)
				card.Enable = false;
			Count--;
		}

		return true;
	}

}