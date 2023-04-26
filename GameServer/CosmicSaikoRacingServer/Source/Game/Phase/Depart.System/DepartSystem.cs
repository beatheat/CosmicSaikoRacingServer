namespace CSRServer.Game
{
	public class DepartSystem
	{
		private readonly Queue<Func<CardEffectModule.Result>> _departEvents;

		public DepartSystem()
		{
			_departEvents = new Queue<Func<CardEffectModule.Result>>();
		}
		
		/// <summary>
		/// 발진 페이즈 발생할 이벤트 추가
		/// </summary>
		public void AddEvent(Func<CardEffectModule.Result> departEvent)
		{
			_departEvents.Enqueue(departEvent);
		}

		public void TurnStart(out CardEffectModule.Result[] attackResult)
		{
			//발진 페이즈 시 발생할 공격정보를 보여준다
			CardEffectModule.Result[] _attackResult = new CardEffectModule.Result[_departEvents.Count];
			for (int idx = 0; _departEvents.Count > 0; idx++)
			{
				var turnEndEvent = _departEvents.Dequeue();
				_attackResult[idx] = turnEndEvent();
			}

			attackResult = _attackResult;
		}
	}
}