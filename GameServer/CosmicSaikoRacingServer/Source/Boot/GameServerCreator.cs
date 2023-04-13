using System.Net;
using System.Net.Sockets;
using EdenNetwork;
using EdenNetwork.Udp;
using ConnectionState = EdenNetwork.ConnectionState;

namespace CSRServer
{
	public class GameServerCreator
	{
		private readonly EdenUdpServer _server;
		private GameManager? _gameManager;
		
		public GameServerCreator(EdenUdpServer server)
		{
			this._server = server;
			this._gameManager = null;
		}

		public void Run()
		{
			// TODO: 이미 접속된 클라이언트의 연결이 끊어진 후 재접속했을 때 정보를 유지해야한다. 
			_server.Listen(1);
			
			_server.AddResponse("CreateGame", CreateGame);
			_server.AddResponse("CreateCustomGame", CreateCustomGame);
			_server.AddReceiveEvent("DestroyGame", DestroyGame);
		}

		public void Close()
		{
			_server?.RemoveResponse("CreateGame");
			_server?.RemoveResponse("CreateCustomGame");
			_server?.RemoveReceiveEvent("DestroyGame");
			
			_server?.Close();
		}

		/// <summary>
		/// 게임 서버 종료
		/// </summary>
		private void DestroyGame(string clientId, EdenData data)
		{
			_gameManager?.Close();
			_gameManager = null;
		}
		
		/// <summary>
		/// 랜덤 매칭 게임 생성
		/// </summary>
		private EdenData CreateGame(string clientId, EdenData data)
		{
			//Scene scene = scenes.Peek();
			//scene.passingData.Add("hostp
			//mNumber", roomNumber);
			//overlayScene = new ChatScene(this, server);
			//overlayScene.Load();
			//return new EdenData(1, "OK");
			Task.Run(() =>
			{
				//while(server.Client)
			});
			return new EdenData();
		}
		
		private EdenData CreateCustomGame(string clientId, EdenData data)
		{
			// 매칭서버에 접속
			EdenUdpClient matchClient = new EdenUdpClient(Program.config.matchingServerAddress, Program.config.matchingServerPort,"MatchNetworkLog.txt");
			
			if (matchClient.Connect() == ConnectionState.OK)
			{
				int gameServerPort = matchClient.GetLocalEndPort();
				// 매칭서버에 로비를 생성하고 방번호를 받는다
				EdenData matchingServerData = matchClient.Request("CreateLobby", GetLocalIPAddress() + ":" + gameServerPort);
				matchClient.Close();
				if (data.type == EdenData.Type.ERROR)
					return new EdenData(new Dictionary<string, object> {["state"] = false, ["message"] = "Cannot Create Game : cannot create lobby"});

				if (!matchingServerData.TryGet<int>(out var roomNumber))
					return new EdenData(new Dictionary<string, object> {["state"] = false, ["message"] = "Cannot Create Game : cannot create lobby"});

				// 게임서버가 존재한다면 닫는다
				_gameManager?.Close();
				// 게임서버를 생성한다.
				EdenUdpServer gameServer = new EdenUdpServer(gameServerPort, Program.config.gameNetworkLogPath);
				gameServer.AddReceiveEvent("HeartBeat", (_, _) => { Logger.Log("asdf"); }, false);
				_gameManager = new GameManager(gameServer);

				var lobbyScene = new LobbyScene(_gameManager, gameServer);
				_gameManager.AddScene(lobbyScene, new GameScene(_gameManager, gameServer));

				// lobbyScene.passingData.Add("hostId", clientId);
				lobbyScene.passingData.Add("roomNumber", roomNumber);
				
				// 게임서버를 실행한다.
				_gameManager.Run();

				// 로비 생성을 성공했음을 클라이언트에 반환한다
				return new EdenData(new Dictionary<string, object> {["state"] = true, ["message"] = "OK", ["port"] = gameServerPort});
			}

			return new EdenData(new Dictionary<string, object> {["state"] = false, ["message"] = "Cannot Create Game : cannot connect Matching Server"});
		}
		
		public static string GetLocalIPAddress()
		{
			var host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (var ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					return ip.ToString();
				}
			}
			throw new Exception("No network adapters with an IPv4 address in the system!");
		}
	}
}