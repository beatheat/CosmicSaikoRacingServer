using CSR.Lobby;
using ProtoBuf;

#pragma warning disable CS8618

namespace CSR.DataTransmission;

[ProtoContract]
public class Request_LobbyLogin
{
	[ProtoMember(1)] 
	public string Nickname { get; set; } = "";
}

[ProtoContract]
public class Response_LobbyLogin : ResponseBase
{
	[ProtoMember(1)]
	public int PlayerId { get; set; }
	[ProtoMember(2)]
	public int LobbyNumber { get; set; }
	[ProtoMember(3)]
	public List<LobbyPlayer> LobbyPlayers { get; set; }
}