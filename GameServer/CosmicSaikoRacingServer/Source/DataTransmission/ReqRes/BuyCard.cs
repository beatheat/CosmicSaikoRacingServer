using CSR.Game.GameObject;
using ProtoBuf;

namespace CSR.DataTransmission;


[ProtoContract]
public class Request_BuyCard
{
    [ProtoMember(1)]
    public int Index { get; set; }
}

[ProtoContract]
public class Response_BuyCard : ResponseBase
{
    [ProtoMember(1)]
    public List<Card> ShopCards { get; set; }
    [ProtoMember(2)]
    public Card BuyCard { get; set; }
    [ProtoMember(3)]
    public List<Card> Deck { get; set; }
    [ProtoMember(4)]
    public int Coin { get; set; }
}