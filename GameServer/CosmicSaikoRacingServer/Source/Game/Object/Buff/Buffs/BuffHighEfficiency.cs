namespace CSRServer.Game
{
	internal class BuffHighEfficiency : Buff
	{
		public BuffHighEfficiency(GamePlayer player) : base(player)
		{
			type = Buff.Type.HighEfficiency;
		}

		/// <summary>
		/// 턴 종료시 고효율 버프수 * 0.1 만큼 더 전진함
		/// </summary>
		public override void OnTurnEnd()
		{
			player.turnDistance = (int) (Math.Round(player.turnDistance * (1.0 + 0.1 * count)));
			base.Release();
		}
	}
}