namespace CSR.Game.GameObject;


public partial class CardEffectModule
{
	/// <summary>
	/// 카드 이펙트 모듈의 종류
	/// </summary>
	public enum Type
	{
		Nothing,
		Add,
		Multiply,
		Draw,
		RerollCountUp,
		Death,
		Fail,
		Initialize,
		ForceReroll,
		CreateCardToHand,
		CreateCardToDeck,
		CreateCardToOther,
		BuffToMe,
		BuffToOther,
		EraseBuff,
		Overload,
		Combo,
		EnforceSelf,
		Discard,
		Choice,
		DoPercent,
		SetVariable,
		Check,
		Leak,
		Repeat,
		TurnEnd
	}

}	
