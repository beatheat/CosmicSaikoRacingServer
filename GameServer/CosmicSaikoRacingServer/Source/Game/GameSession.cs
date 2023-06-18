using CSR.Game.Phase;
using CSR.Home;
using CSR.Lobby;
using EdenNetwork;

namespace CSR.Game.GameObject;

public class GameSession : SessionBase
{
	public List<GamePlayer> PlayerList { get; }
	public int Turn { get; private set; }

	private PreheatPhase _preheatPhase;
	private DepartPhase _departPhase;
	private MaintainPhase _maintainPhase;
	
	public GameSession(SessionManager sessionManager, IEdenNetServer server, List<LobbyPlayer> lobbyPlayers) : base(sessionManager, server)
	{
		PlayerList = new List<GamePlayer>();
		Turn = 1;

		_preheatPhase = new PreheatPhase(this, server);
		_departPhase = new DepartPhase(this, server);
		_maintainPhase = new MaintainPhase(this, server);
		
		foreach(LobbyPlayer lbPlayer in lobbyPlayers)
		{
			var gamePlayer = new GamePlayer(lbPlayer.ClientId, PlayerList.Count, lbPlayer.Nickname, PlayerList, _preheatPhase);
			PlayerList.Add(gamePlayer);
		}
	}

	public override void Load()
	{
		server.AddEndpoints(this);
		server.BroadcastAsync("LobbyGameStart");
	}
	
	public override void Destroy()
	{
		server.RemoveEndpoints(this);
	}
        
	public void PreheatStart()
	{
		Turn++;
		_preheatPhase.PreheatStart();
	}

	public void DepartStart()
	{
		_departPhase.DepartStart();
	}

	public void MaintainStart()
	{
		_maintainPhase.MaintainStart();
	}
        
	/// <summary>
	/// 클라이언트 플레이어가 본인의 플레이어 정보 외 다른 플레이어 정보를 원할때 보내주는 플레이어 리스트를 반환한다
	/// </summary>
	public List<GamePlayer> GetMonitorPlayerList()
	{
		List<GamePlayer> monitorPlayerList = new List<GamePlayer>();
		foreach (var player in PlayerList)
		{
			monitorPlayerList.Add(player.CloneForMonitor());
		}
		return monitorPlayerList;
	}

	
	[EdenReceive]
	private void PlayerReady(PeerId clientId)
	{
		var player = PlayerList.Find(player => player.clientId == clientId);
		if (player == null)
			return;
		player.PhaseReady = true;
		bool gameStart = true;
		
		foreach (var p in PlayerList)
		{
			gameStart = gameStart && p.PhaseReady;
		}

		if (gameStart)
		{
			Turn = 1;
			PreheatStart();
		}
	}

	[EdenReceive]
	private void DestroyGame(PeerId clientId)
	{
		var player = PlayerList.Find(player => player.clientId == clientId);
		if (player == null) return;
		// 호스트인지 체크
		if (player.Index == 0)
		{
			sessionManager.ChangeSession<HomeSession>();
		}
	}
}