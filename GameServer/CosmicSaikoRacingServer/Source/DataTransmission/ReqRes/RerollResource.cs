using CSR.Game.Player;
using ProtoBuf;

namespace CSR.DataTransmission;

[ProtoContract]
public class Request_RerollResource
{
	public List<int> ResourceFixed { get; set; }
}

[ProtoContract]
public class Response_RerollResource : ResponseBase
{
	[ProtoMember(1)]
	public GamePlayer Player { get; set; }	
}