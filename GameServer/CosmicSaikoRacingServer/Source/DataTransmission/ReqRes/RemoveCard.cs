using CSR.Game.GameObject;
using ProtoBuf;

#pragma warning disable CS8618

namespace CSR.DataTransmission;

[ProtoContract]
public class Request_RemoveCard
{
    [ProtoMember(1)]
    public int Index { get; set; } = -1;
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