using CSR.Game;
using CSR.Game.GameObject;
using ProtoBuf;

#pragma warning disable CS8618

namespace CSR.DataTransmission;

[ProtoContract]
public class Request_UseCard
{
	[ProtoMember(1)] 
	public int Index { get; set; } = -1;
}

[ProtoContract]
public class Response_UseCard : ResponseBase
{
	[ProtoMember(1)]
	public GamePlayer Player { get; set; }
	[ProtoMember(2)]
	public CardEffect.Result Result { get; set; }
}