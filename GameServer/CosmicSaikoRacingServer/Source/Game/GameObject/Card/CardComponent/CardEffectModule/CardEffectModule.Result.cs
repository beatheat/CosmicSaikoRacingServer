using CSR.Game.GameObject.CardEffectModuleImplementation;
using ProtoBuf;

namespace CSR.Game.GameObject;

public partial class CardEffectModule
{
	/// <summary>
	/// 카드 이펙트모듈의 결과값 클래스
	/// </summary>
	[ProtoContract]
	[ProtoInclude(10, typeof(Add.ResultAdd))]
	[ProtoInclude(11, typeof(BuffToMe.ResultBuffToMe))]
	[ProtoInclude(12, typeof(BuffToOther.ResultBuffToOther))]
	[ProtoInclude(13, typeof(Choice.ResultChoice))]
	[ProtoInclude(14, typeof(Combo.ResultCombo))]
	[ProtoInclude(15, typeof(CreateCardToDeck.ResultCreateCardToDeck))]
	[ProtoInclude(16, typeof(CreateCardToHand.ResultCreateCardToHand))]
	[ProtoInclude(17, typeof(CreateCardToOther.ResultCreateCardToOther))]
	[ProtoInclude(18, typeof(Discard.ResultDiscard))]
	[ProtoInclude(19, typeof(Draw.ResultDraw))]
	[ProtoInclude(20, typeof(EraseBuff.ResultEraseBuff))]
	[ProtoInclude(21, typeof(ForceReroll.ResultForceReroll))]
	[ProtoInclude(22, typeof(Leak.ResultLeak))]
	[ProtoInclude(23, typeof(Multiply.ResultMultiply))]
	[ProtoInclude(24, typeof(Overload.ResultOverload))]
	[ProtoInclude(25, typeof(Repeat.ResultRepeat))]
	[ProtoInclude(26, typeof(RerollCountUp.ResultRerollCountUp))]		
	public class Result
	{
		[ProtoMember(1)]
		public Type Type { get; set; }
	}
}
