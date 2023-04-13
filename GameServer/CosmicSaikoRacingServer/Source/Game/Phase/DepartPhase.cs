﻿using EdenNetwork;
using EdenNetwork.Udp;

namespace CSRServer.Game
{
	public class DepartPhase
	{
		private const int INITIAL_TIME = 99;
		private readonly TurnData _turnData;
		private readonly GameManager _gameManager;
		private readonly EdenUdpServer _server;
		private readonly GameScene _parent;

		private Timer? _timer;
		private int _time;
		private bool _turnEnd;

		public DepartPhase(GameManager gameManager, EdenUdpServer server, TurnData turnData, GameScene parent)
		{
			this._gameManager = gameManager;
			this._server = server;
			this._turnData = turnData;
			this._parent = parent;
			this._timer = null;
			this._time = 0;
			this._turnEnd = false;
		}
		
		
		// 발진 페이즈 타이머
		private async void GameTimer(object? sender)
		{
			if (_time >= 0)
			{
				_time--;
				await _server.BroadcastAsync("MaintainTime", _time, log: false);
			}
			else
			{
				DepartEnd();
			}
		}
		
		/// <summary>
		/// 발진 페이즈 시작
		/// </summary>
		public void DepartStart()
		{
			_server.AddReceiveEvent("DepartReady", DepartReady);
			
			_time = INITIAL_TIME;
			_turnEnd = false;
			List<CardEffectModule.Result> attackResults = new List<CardEffectModule.Result>();
            
			foreach (var player in _turnData.playerList)
			{
				player.DepartStart(out var attackResult);
				attackResults.AddRange(attackResult);
				// obstacleResults.AddRange(obstacleResult);
				player.phaseReady = false;
			}
			
			//발진 페이즈 데이터 클라이언트와 동기화
			_server.Broadcast("DepartStart", new Dictionary<string, object>
			{
				["playerList"] = _parent.GetMonitorPlayerList(),
				["attackResults"] = attackResults,
			});
			
			_timer = new Timer(GameTimer, null, 0, 1000);
		}

		/// <summary>
		/// 발진 페이즈 종료
		/// </summary>
		private void DepartEnd()
		{
			lock (this)
			{
				if (_turnEnd == false)
				{
					_timer?.Dispose();
					_server.RemoveReceiveEvent("DepartReady");
					_parent.MaintainStart();
					_turnEnd = true;
				}
			}
		}

		#region Receive/Response Methods
		/// <summary>
		/// 발진 페이즈 준비완료 API
		/// </summary>
		private void DepartReady(string clientId, EdenData data)
		{
			GamePlayer player = _turnData.playerMap[clientId];
			player.phaseReady = true;

			//모든 플레이어가 예열턴을 마쳤는지 체크
			bool checkAllReady = true;
			foreach (var p in _turnData.playerList)
				checkAllReady = checkAllReady && p.phaseReady;
			if (checkAllReady)
				DepartEnd();
		}
		#endregion

	}
}