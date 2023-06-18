using CSR.Game.GameObject;

namespace CSR.Game;


public static class DepartLogic
{
	public static void InitDepart(this GamePlayer player)
	{
		player.Depart.events = new Queue<DepartEvent>();
	}

	/// <summary>
	/// 발진 페이즈 발생할 이벤트 추가
	/// </summary>
	public static void AddDepartEvent(this GamePlayer player, DepartEvent departEvent)
	{
		player.Depart.events.Enqueue(departEvent);
	}

	public static void DepartOnTurnStart(this GamePlayer player, out CardEffect.Result attackResult)
	{
		//발진 페이즈 시 발생할 공격정보를 보여준다
		var turnEndEventResults = new List<CardEffectModule.Result>();
		for (int idx = 0; player.Depart.events.Count > 0; idx++)
		{
			var turnEndEvent = player.Depart.events.Dequeue();
			turnEndEventResults.Add(turnEndEvent());
		}

		attackResult = new CardEffect.Result(turnEndEventResults);
	}
}
