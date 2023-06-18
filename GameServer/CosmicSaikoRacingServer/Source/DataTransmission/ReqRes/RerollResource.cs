using CSR.Game;
using ProtoBuf;

#pragma warning disable CS8618

namespace CSR.DataTransmission;

[ProtoContract]
public class Request_RerollResource
{
	[ProtoMember(1)]
	public List<int> ResourceFixed { get; set; } = new List<int>();
}

[ProtoContract]
public class Response_RerollResource : ResponseBase
{
	[ProtoMember(1)]
	public GamePlayer Player { get; set; }	
}