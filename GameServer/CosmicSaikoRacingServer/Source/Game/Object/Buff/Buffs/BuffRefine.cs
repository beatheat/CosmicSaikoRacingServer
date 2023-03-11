namespace CSRServer.Game
{
	internal class BuffRefine : Buff
	{
		public BuffRefine(GamePlayer player) : base(player)
		{
			type = Buff.Type.Refine;
		}

		public override void OnRollResource(ref List<int>? resourceFixed)
		{
			base.OnRollResource(ref resourceFixed);
		}

		public override void OnTurnEnd()
		{
			
		}
	}
}