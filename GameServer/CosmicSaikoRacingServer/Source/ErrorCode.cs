namespace CSR;

public enum ErrorCode : ushort
{
	// Global 0 ~ 99
	None = 0,
	PlayerNotExist = 1,
	PlayerTurnEnd = 2,
	MissingPacketData = 3,
	
	
	// PreheatPhase 100 ~ 199
	// UseCard 110 ~
	UseCard_ResourceConditionNotSatisfy = 110,
	UseCard_WrongIndex = 111,
	UseCard_CardRestrictedByBuff = 112,
	
	
	//RerollResource 120 ~ 
	RerollResource_RerollCountZero = 120,
	
	//DepartPhase 200 ~ 299
	
	//MaintainPhase 300 ~ 399
	MaintainPhase_CoinNotEnough = 300,
	MaintainPhase_MaxLevel = 301,

	//BuyCard 310 ~
	BuyCard_WrongIndex = 310,

	//RemoveCard 320 ~
	RemoveCard_WrongIndex = 320,
	
}