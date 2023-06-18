using CSR.Game.GameObject;
using ProtoBuf;

#pragma warning disable CS8618

namespace CSR.DataTransmission;

[ProtoContract]
public class Packet_MaintainStart
{
	[ProtoMember(1)]
	public List<Card> ShopCards { get; set; }
	[ProtoMember(2)] 
	public List<Card> RemoveCards { get; set; }
}