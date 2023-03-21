namespace CSRServer.Game
{
	internal class BuffHighDensity : Buff
	{
		public BuffHighDensity(GamePlayer player) : base(player)
		{
			type = Buff.Type.HighDensity;
		}

		/// <summary>
		/// 고밀도 버프가 있을 경우 카드가 두번 발동함
		/// </summary>
		public override void AfterUseCard(ref Card card, ref CardEffectModule.Result[] results)
		{
			if (count > 0)
			{
				results = results.Concat(card.UseEffect(player)).ToArray();
				count--;
			}
		}
	}
}