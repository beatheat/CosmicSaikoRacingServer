using ProtoBuf;

#pragma warning disable CS8618

namespace CSR.DataTransmission;


[ProtoContract]
public class Request_CreateCustomGame
{

}

[ProtoContract]
public class Response_CreateCustomGame : ResponseBase
{
	[ProtoMember(1)]
	public string Message { get; set; }
}