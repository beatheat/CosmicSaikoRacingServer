using CSR.Game;
using ProtoBuf;

#pragma warning disable CS8618

namespace CSR.DataTransmission;

[ProtoContract]
public class Packet_PreheatStart
{
	[ProtoMember(1)] 
	public GamePlayer Player { get; set; } = null!;
	[ProtoMember(2)] 
	public List<GamePlayer> PlayerList { get; set; } = new();
	[ProtoMember(3)]
	public int Turn { get; set; }
	[ProtoMember(4)]
	public int Timer { get; set; }
}