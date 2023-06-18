using ProtoBuf;


namespace CSR.DataTransmission;

[ProtoContract]
public class Request_BuyExp
{

}

[ProtoContract]
public class Response_BuyExp : ResponseBase
{
    [ProtoMember(1)]
    public int Level { get; set; }
    [ProtoMember(2)]
    public int Exp { get; set; }
    [ProtoMember(3)]
    public int Coin { get; set; }
}