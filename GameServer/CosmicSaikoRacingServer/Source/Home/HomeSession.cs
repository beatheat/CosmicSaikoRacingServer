using CSR.DataTransmission;
using CSR.Lobby;
using EdenNetwork;
using EdenNetwork.EdenException;

namespace CSR.Home;

public class HomeSession : SessionBase
{
	private readonly EdenUdpClient _matchClient;

	public HomeSession(SessionManager manager, IEdenNetServer server) : base(manager, server)
	{
		_matchClient = new EdenUdpClient(ConfigManager.Config.MatchingServerAddress, ConfigManager.Config.MatchingServerPort);
	}

	public override void Load()
	{
		server.AddEndpoints(this);	
		// 새 게임 시작시 가비지 수집
		System.GC.Collect();
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
	public Response_CreateCustomGame CreateCustomGame(PeerId peerId)
	{
		if (_matchClient.Connect() == ConnectionState.Ok)
		{
			int lobbyNumber;
			try
			{
				lobbyNumber = _matchClient.Request<int>("CreateLobby");
			}
			catch (EdenTimeoutException)
			{
				return new Response_CreateCustomGame {ErrorCode = ErrorCode.MatchingServerNotReady, Message = "MatchingServer Timeout"};
			}
			catch(Exception e)
			{
				return new Response_CreateCustomGame {ErrorCode = ErrorCode.MatchingServerNotReady, Message = e.Message};
			}

			if (lobbyNumber < 0)
				return new Response_CreateCustomGame {ErrorCode = ErrorCode.NoRoomRemain};
			
			((EdenUdpServer)server).RegisterNatHolePunching(ConfigManager.Config.MatchingServerAddress, ConfigManager.Config.MatchingServerPort, $"host/{lobbyNumber}");
			sessionManager.ChangeSession<LobbySession>(lobbyNumber);
		
			return new Response_CreateCustomGame {Message = "OK"};
		}
		return new Response_CreateCustomGame {ErrorCode = ErrorCode.MatchingServerNotReady, Message = "Cannot Connect to MatchingServer"};
	}
}