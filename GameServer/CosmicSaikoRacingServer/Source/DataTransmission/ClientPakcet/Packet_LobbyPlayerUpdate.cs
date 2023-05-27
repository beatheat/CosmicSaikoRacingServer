using CSR.Lobby;
using ProtoBuf;

namespace CSR.DataTransmission;

[ProtoContract]
public class Packet_LobbyPlayerUpdate
{
	[ProtoMember(1)]
	public List<LobbyPlayer> LobbyPlayers { get; set; }
}