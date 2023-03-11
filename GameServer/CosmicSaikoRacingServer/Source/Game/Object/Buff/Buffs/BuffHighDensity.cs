namespace CSRServer.Game
{
	internal class BuffHighDensity : Buff
	{
		public BuffHighDensity(GamePlayer player) : base(player)
		{
			type = Buff.Type.HighDensity;
		}

		public override void AfterUseCard(ref Card card, ref CardEffect.Result[] results)
		{
			if (count > 0)
			{
				results = results.Concat(card.UseEffect(player)).ToArray();
				count--;
			}
		}
	}
}