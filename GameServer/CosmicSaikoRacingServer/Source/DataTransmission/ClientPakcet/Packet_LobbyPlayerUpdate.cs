using CSR.Lobby;
using ProtoBuf;

#pragma warning disable CS8618


namespace CSR.DataTransmission;

[ProtoContract]
public class Packet_LobbyPlayerUpdate
{
	[ProtoMember(1)] 
	public List<LobbyPlayer> LobbyPlayers { get; set; }
}