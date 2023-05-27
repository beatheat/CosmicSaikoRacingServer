using CSR.Game.GameObject;
using ProtoBuf;

namespace CSR.DataTransmission;

[ProtoContract]
public class Request_RemoveCard
{
    [ProtoMember(1)]
    public int Index { get; set; }
}

[ProtoContract]
public class Response_RemoveCard : ResponseBase
{
    [ProtoMember(1)]
    public List<Card> RemoveCards { get; set; }
    [ProtoMember(2)]
    public List<Card> Deck { get; set; }
    [ProtoMember(3)]
    public int Coin { get; set; }
}