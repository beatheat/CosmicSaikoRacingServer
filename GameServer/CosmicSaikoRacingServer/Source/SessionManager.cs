using CSR.Home;
using EdenNetwork;

namespace CSR;

public class SessionManager
{
	private const int MAX_PLAYER_NUMBER = 4;
	
	private readonly EdenUdpServer _server;

	private SessionBase _currentSession;
	
	public SessionManager()
	{
		_server = new EdenUdpServer(ConfigManager.Config.GameServerPort);
	}
	
	public void Run<T>(params object[] parameters) where T : SessionBase
	{
		// ConfigManager.LoadConfig();

		var session = CreateSession<T>(parameters);
		
		_server.Listen(MAX_PLAYER_NUMBER);

		_currentSession = session;
		
		_currentSession.Load();
	}

	public void ChangeSession<T>(params object[] parameters) where T : SessionBase
	{
		var session = CreateSession<T>(parameters);
		_currentSession.Destroy();
		_currentSession = session;
		_currentSession.Load();
	}
	
	public void Close()
	{
		_currentSession.Destroy();
		_server.Close();
	}

	private SessionBase CreateSession<T>(params object[] parameters) where T : SessionBase
	{
		var parameterTypes = new[] {typeof(SessionManager), typeof(EdenUdpServer)};
		
		foreach (var param in parameters)
		{
			parameterTypes = parameterTypes.Append(param.GetType()).ToArray();
		}
		
		var sessionConstructorInfo = typeof(T).GetConstructor(parameterTypes);
		if (sessionConstructorInfo == null)
		{
			throw new Exception();
		}

		parameters = new object[] {this, _server}.Concat(parameters).ToArray();

		var session = sessionConstructorInfo.Invoke(parameters) as SessionBase;
		if (session == null)
		{
			throw new Exception();
		}

		return session;
	}
}
