using EdenNetwork;

namespace CSR;

public abstract class SessionBase
{
	protected readonly SessionManager sessionManager;
	protected readonly IEdenNetServer server;

	protected SessionBase(SessionManager sessionManager, IEdenNetServer server)
	{
		this.sessionManager = sessionManager;
		this.server = server;
	}
	
	//세션이 시작할 때 실행하는 메소드
	public virtual void Load() { } 
	//세션이 종료할 때 실행하는 메소드
	public virtual void Destroy() { }
}