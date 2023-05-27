using CSR.Game.GameObject;
using CSR.Game.Phase;
using CSR.Game.Player;
using ProtoBuf;

namespace CSR;


[ProtoContract]
internal class BuffHighEfficiency : Buff
{
	public BuffHighEfficiency(PreheatPhase phase, GamePlayer player) : base(phase, player)
	{
		Type = BuffType.HighEfficiency;
	}

	/// <summary>
	/// 턴 종료시 고효율 버프수 * 0.1 만큼 더 전진함
	/// </summary>
	public override void OnDepartStart()
	{
		player.TurnDistance = (int) (Math.Round(player.TurnDistance * (1.0 + 0.1 * Count)));
		base.Release();
	}

	public override void OnPreheatEnd()
	{
	}
}