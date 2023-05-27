using CSR.Game.GameObject;
using CSR.Game.Player;
using ProtoBuf;

namespace CSR.DataTransmission;

[ProtoContract]
public class Request_UseCard
{
	[ProtoMember(1)]
	public int Index { get; set; }
}

[ProtoContract]
public class Response_UseCard : ResponseBase
{
	[ProtoMember(1)]
	public GamePlayer Player { get; set; }
	[ProtoMember(2)]
	public CardEffectModule.Result[] Results { get; set; }
}