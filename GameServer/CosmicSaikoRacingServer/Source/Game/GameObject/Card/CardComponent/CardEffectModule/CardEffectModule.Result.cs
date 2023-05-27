using ProtoBuf;

namespace CSR.Game.GameObject;

public partial class CardEffectModule
{
	/// <summary>
	/// 카드 이펙트모듈의 결과값 클래스
	/// </summary>
	[ProtoContract]
	public class Result
	{
		[ProtoMember(1)]
		public object? Value { get; set; }
		[ProtoMember(2)]
		public Type Type { get; set; }
	}
}
