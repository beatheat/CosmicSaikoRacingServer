using CSR.Game.GameObject;
using ProtoBuf;

namespace CSR.Game.Player;

[ProtoContract]
public class CardData
{
	[ProtoMember(1)]
	public List<Card> Hand { get; set; }
	[ProtoMember(2)]
	public List<Card> Deck { get; set; }
	[ProtoMember(3)]
	public List<Card> Used { get; set; }
	[ProtoMember(4)]
	public List<Card> Unused { get; set; }
	[ProtoMember(5)]
	public List<Card> TurnUsed { get; set; }
	[ProtoMember(6)]
	public int DrawCount { get; set; }
}