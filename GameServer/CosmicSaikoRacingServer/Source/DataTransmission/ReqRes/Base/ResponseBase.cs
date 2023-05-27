using ProtoBuf;

namespace CSR.DataTransmission;

[ProtoContract]
public class ResponseBase
{
	[ProtoMember(100)]
	public ErrorCode ErrorCode { get; set; } = ErrorCode.None;
}