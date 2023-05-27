using CSR.Lobby;
using EdenNetwork;

namespace CSR.Home;

public class HomeSession : SessionBase
{
	private EdenUdpClient _matchClient;

	public HomeSession(SessionManager manager, EdenUdpServer server) : base(manager, server)
	{
		_matchClient = new EdenUdpClient(ConfigManager.Config.MatchingServerAddress, ConfigManager.Config.MatchingServerPort);
	}

	public override void Load()
	{
		server.AddEndpoints(this);
	}

	public override void Destroy()
	{
		server.RemoveEndpoints(this);
	}

	[EdenResponse]
	public void CreateRandomMatchGame(PeerId peerId)
	{
		
	}
	
	[EdenResponse]
	public void CreateCustomGame(PeerId peerId)
	{
		if (_matchClient.Connect() == ConnectionState.OK)
		{
			int lobbyNumber;
			try
			{
				lobbyNumber = _matchClient.Request<int>("CreateLobby");
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
				throw new Exception();
			}

			server.RegisterNatHolePunching(ConfigManager.Config.MatchingServerAddress, ConfigManager.Config.MatchingServerPort, $"host/{lobbyNumber}");
			sessionManager.ChangeSession<LobbySession>(_matchClient, lobbyNumber);
		}
	}
}