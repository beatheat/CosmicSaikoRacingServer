namespace CSRServer.Game
{
	internal class BuffHighEfficiency : Buff
	{
		public BuffHighEfficiency(GamePlayer player) : base(player)
		{
			type = Buff.Type.HighEfficiency;
		}

		public override void OnTurnEnd()
		{
			player.turnDistance = (int) (player.turnDistance * (1.0 + 0.1 * count));
			base.Release();
		}
	}
}