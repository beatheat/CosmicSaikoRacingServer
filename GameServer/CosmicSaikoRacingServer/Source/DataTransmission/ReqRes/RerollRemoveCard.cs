using CSR.Game.GameObject;
using ProtoBuf;

namespace CSR.DataTransmission;

[ProtoContract]
public class Request_RerollRemoveCard
{
}

[ProtoContract]
public class Response_RerollRemoveCard : ResponseBase
{
    [ProtoMember(1)]
    public int Coin { get; set; }
    [ProtoMember(2)]
    public List<Card> RemoveCards { get; set; }
}