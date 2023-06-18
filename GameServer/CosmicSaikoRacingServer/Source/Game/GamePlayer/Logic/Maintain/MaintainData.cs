using CSR.Game.GameObject;
using ProtoBuf;

#pragma warning disable CS8618

namespace CSR.Game;

[ProtoContract]
public class MaintainData
{
	//코인
	[ProtoMember(1)]
	public int Coin { get; set; }
	//턴 당 시작 코인 
	[ProtoMember(2)]
	public int TurnCoinCount { get; set; }
	//상점 카드
	[ProtoMember(3)]
	public List<Card> ShopCards { get; set; }
	//제거 카드
	[ProtoMember(4)]
	public List<Card> RemoveCards { get; set; }
	
}