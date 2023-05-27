using ProtoBuf;

namespace CSR.DataTransmission;

[ProtoContract]
public class Packet_LobbyPlayerReady
{
	[ProtoMember(1)]
	public int PlayerId { get; set; }
	[ProtoMember(2)]
	public bool ReadyState { get; set; }
}