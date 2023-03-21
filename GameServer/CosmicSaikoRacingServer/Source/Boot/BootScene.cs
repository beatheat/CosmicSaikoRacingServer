using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdenNetwork;

namespace CSRServer.Lobby
{
	public class BootScene : Scene
	{

		public BootScene(GameManager gameManager, EdenNetServer server) : base(gameManager, server)
		{

		}

		public override void Load()
		{
			server.AddResponse("CreateGame", CreateGame);
			server.AddResponse("CreateCustomGame", CreateCustomGame);
		}

		public override void Destroy()
		{
			server.RemoveResponse("CreateGame");
			server.RemoveResponse("CreateCustomGame");
		}

		#region Receive/Response Methods
		
		/// <summary>
		/// 랜덤 매칭 게임 생성
		/// </summary>
		private EdenData CreateGame(string clientId, EdenData data)
		{
			//Scene scene = scenes.Peek();
			//scene.passingData.Add("hostplayer", new LobbyPlayer(client_id, d.Get<string>(), 0, true));
			//scene.passingData.Add("roomNumber", roomNumber);
			//overlayScene = new ChatScene(this, server);
			//overlayScene.Load();
			//return new EdenData(1, "OK");
			Task.Run(() =>
			{
				//while(server.Client)
			});
			return new EdenData();
		}

		/// <summary>
		/// 커스텀 게임 생성
		/// </summary>
		private EdenData CreateCustomGame(string clientId, EdenData data)
		{
			// 매칭서버에 접속
			EdenNetClient client = new EdenNetClient(Program.config.matchingServerAddress, Program.config.matchingServerPort);
			if (client.Connect() == ConnectionState.OK)
			{
				// 매칭서버에 로비를 생성하고 방번호를 받는다
				EdenData matchingServerData = client.Request("CreateLobby", 5, Program.config.port);
				client.Close();
				if (data.type == EdenData.Type.ERROR)
					return new EdenData(new Dictionary<string, object> {["state"] = false, ["message"] = "Cannot Create Game : cannot create lobby"});

				if (!matchingServerData.TryGet<int>(out var roomNumber))
					return new EdenData(new Dictionary<string, object> {["state"] = false, ["message"] = "Cannot Create Game : cannot create lobby"});

				gameManager.AddScene(new LobbyScene(gameManager, server));
				gameManager.AddScene(new GameScene(gameManager, server));
				
				this.passingData.Add("hostId", clientId);
				this.passingData.Add("roomNumber", roomNumber);
				
				// 로비씬으로 바꾼다
				gameManager.ChangeToNextScene();

				// 로비 생성을 성공했음을 클라이언트에 반환한다
				return new EdenData(new Dictionary<string, object> {["state"] = true, ["message"] = "OK"});
			}

			return new EdenData(new Dictionary<string, object> {["state"] = false, ["message"] = "Cannot Create Game : cannot connect Matching Server"});
		}
		#endregion

	}
}