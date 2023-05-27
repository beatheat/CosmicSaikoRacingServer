using CSR.DataTransmission;
using CSR.Game.GameObject;
using CSR.Game.Player;
using EdenNetwork;

namespace CSR.Game.Phase;

public class DepartPhase
{
    private const int INITIAL_TIME = 99;
    private readonly GameSession _parent;
    private readonly EdenUdpServer _server;

    private Timer? _timer;
    private int _time;
    private bool _turnEnd;

	public DepartPhase(GameSession parent, EdenUdpServer server)
	{
		this._server = server;
		this._parent = parent;
		this._timer = null;
		this._time = 0;
		this._turnEnd = false;
	}
	
	// 발진 페이즈 타이머
	private async void GameTimer(object? sender)
	{
		if (_time > 0)
		{
			_time--;
			await _server.BroadcastAsync("MaintainTime", _time);
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
		_server.AddEndpoints(this);
		
		_time = INITIAL_TIME;
		_turnEnd = false;
		var attackResults = new List<CardEffectModule.Result>();

		foreach (var player in _parent.PlayerList)
		{
			player.DepartStart(out var attackResult);
			attackResults.AddRange(attackResult);
		}

		
		var monitorPlayerList = _parent.GetMonitorPlayerList();
		foreach (var player in _parent.PlayerList)
		{
			_server.Send("DepartStart", player.ClientId, new Packet_DepartStart
			{
				PlayerList = _parent.GetMonitorPlayerList(),
				AttackResults = attackResults
			});

		}
		//발진 페이즈 데이터 클라이언트와 동기화

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
				_server.RemoveEndpoints(this);
				_parent.MaintainStart();
				_turnEnd = true;
			}
		}
	}

	#region Receive/Response Methods

	/// <summary>
	/// 발진 페이즈 준비완료 API
	/// </summary>
	[EdenReceive]
	private void DepartReady(PeerId clientId)
	{
		var player = _parent.PlayerList.Find(player => player.ClientId == clientId);
		if (player == null)
			return;
		player.PhaseReady = true;

		//모든 플레이어가 발진을 마쳤는지 체크
		bool checkAllReady = true;
		foreach (var p in _parent.PlayerList)
			checkAllReady = checkAllReady && p.PhaseReady;
		if (checkAllReady)
			DepartEnd();
	}

	#endregion
}