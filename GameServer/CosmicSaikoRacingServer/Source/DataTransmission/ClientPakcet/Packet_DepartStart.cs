using CSR.Game;
using CSR.Game.GameObject;
using ProtoBuf;

#pragma warning disable CS8618

namespace CSR.DataTransmission;

[ProtoContract]
public class Packet_DepartStart
{
	[ProtoMember(1)] 
	public List<GamePlayer> PlayerList { get; set; }
	[ProtoMember(2)] 
	public CardEffect.Result AttackResult { get; set; }
}