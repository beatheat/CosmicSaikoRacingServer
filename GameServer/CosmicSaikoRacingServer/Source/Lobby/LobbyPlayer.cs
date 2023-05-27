using EdenNetwork;
using ProtoBuf;

namespace CSR.Lobby;

[ProtoContract]
public class LobbyPlayer
{
	public PeerId ClientId { get; set; }

	[ProtoMember(1)]
	public int Id { get; set; }
	
	[ProtoMember(2)]
	public string Nickname { get; set; }
	
	[ProtoMember(3)]
	public bool IsReady { get; set; }
	
	[ProtoMember(4)]
	public bool Host { get; set; }
	
	
	public LobbyPlayer(PeerId clientId, string nickname, bool host = false)
	{
		this.ClientId = clientId;
		this.Id = clientId.GetHashCode();
		this.Nickname = nickname;
		this.IsReady = false;
		this.Host = host;
	}
}