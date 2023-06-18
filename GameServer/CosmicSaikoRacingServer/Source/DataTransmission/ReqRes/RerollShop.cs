using CSR.Game.GameObject;
using ProtoBuf;

#pragma warning disable CS8618

namespace CSR.DataTransmission;

[ProtoContract]
public class Request_RerollShop
{
	
}

[ProtoContract]
public class Response_RerollShop : ResponseBase
{
	[ProtoMember(1)]
	public int Coin { get; set; }
	[ProtoMember(2)]
	public List<Card> ShopCards { get; set; }
}