using ProtoBuf;


namespace CSR.DataTransmission;

[ProtoContract]
public class Packet_LobbyReady
{
	[ProtoMember(1)] 
	public bool IsReady { get; set; } = false;
}