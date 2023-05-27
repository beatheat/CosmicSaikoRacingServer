using CSR.Game.Player;
using ProtoBuf;

namespace CSR.DataTransmission;

[ProtoContract]
public class Packet_PreheatStart
{
	[ProtoMember(1)]
	public GamePlayer Player { get; set; }
	[ProtoMember(2)]
	public List<GamePlayer> PlayerList { get; set; }
	[ProtoMember(3)]
	public int Turn { get; set; }
	[ProtoMember(4)]
	public int Timer { get; set; }
}