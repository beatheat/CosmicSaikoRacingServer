namespace CSRServer.Game
{
	internal class BuffBreakDown : Buff
	{
		public BuffBreakDown(GamePlayer player) : base(player)
		{
			type = Buff.Type.BreakDown;
		}
		
		public override bool BeforeUseCard(ref Card card)
		{
			//고장(BREAK_DOWN)버프
			if (count > 0)
			{
				Random random = new Random();
				if (random.Next(2) != 0)
					card.enable = false;
				count--;
			}
			return true;
		}

	}
}