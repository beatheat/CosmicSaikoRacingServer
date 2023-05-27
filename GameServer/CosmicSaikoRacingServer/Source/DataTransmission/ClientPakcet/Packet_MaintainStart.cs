using CSR.Game.GameObject;
using ProtoBuf;

namespace CSR.DataTransmission;

[ProtoContract]
public class Packet_MaintainStart
{
	[ProtoMember(1)]
	public List<Card> ShopCards { get; set; }
	[ProtoMember(2)]
	public List<Card> RemoveCards { get; set; }
}