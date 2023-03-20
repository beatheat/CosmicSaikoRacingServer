using EdenNetwork;

namespace CSRServer.Game
{

	internal class DepartPhase
	{
		private const int INITIAL_TIME = 99;
		private readonly TurnData _turnData;
		private readonly GameManager _gameManager;
		private readonly EdenNetServer _server;
		private readonly GameScene _parent;

		private Timer? _timer;
		private int _time;

		public DepartPhase(GameManager gameManager, EdenNetServer server, TurnData turnData, GameScene parent)
		{
			this._gameManager = gameManager;
			this._server = server;
			this._turnData = turnData;
			this._parent = parent;
			this._timer = null;
			this._time = 0;
		}
		
		
		// 1초에 한번씩 실행함
		private void GameTimer(object? sender)
		{
			if (_time >= 0)
			{
				_time--;
				_server.BroadcastAsync("MaintainTime", _time);
			}
			else
			{
				_parent.MaintainStart();
			}
		}
		
		public void DepartStart()
		{
			_server.AddReceiveEvent("DepartReady", DepartReady);
			
			_time = INITIAL_TIME;
			List<CardEffect.Result> attackResults = new List<CardEffect.Result>();
            
			foreach (var player in _turnData.playerList)
			{
				player.PreheatEnd(out var attackResult);
				attackResults.AddRange(attackResult);
				// obstacleResults.AddRange(obstacleResult);
				player.turnReady = false;
			}
			
			_server.BroadcastAsync("DepartStart", new Dictionary<string, object>
			{
				["playerList"] = _parent.GetMonitorPlayerList(),
				["attackResults"] = attackResults,
			});
			
			_timer = new Timer(GameTimer, null, 0, 1000);
		}

		private void DepartEnd()
		{
			_timer?.Dispose();
			_server.RemoveReceiveEvent("DepartReady");
			_parent.MaintainStart();
		}
		
		private void DepartReady(string clientId, EdenData data)
		{
			GamePlayer player = _turnData.playerMap[clientId];
			player.turnReady = true;

			//모든 플레이어가 예열턴을 마쳤는지 체크
			bool checkAllReady = true;
			foreach (var p in _turnData.playerList)
				checkAllReady = checkAllReady && p.turnReady;
			if (checkAllReady)
				DepartEnd();
		}
	}
}