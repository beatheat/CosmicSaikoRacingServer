using CSR.Game.GameObject;
using ProtoBuf;

#pragma warning disable CS8618

namespace CSR.Game;

[ProtoContract]
public class ResourceData
{
	//리소스릴
	[ProtoMember(1)]
	public List<ResourceType> Reel { get; set; }
	//리소스릴의 리롤 카운트
	[ProtoMember(2)]
	public int RerollCount { get; set; }
	//턴 시작 시 부여받는 리롤 카운트
	[ProtoMember(3)]
	public int AvailableRerollCount { get; set; }
	//리소스 릴 카운트
	[ProtoMember(4)]
	public int ReelCount { get; set; }
}