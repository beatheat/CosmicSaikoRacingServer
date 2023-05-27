using CSR.Game.Player;
using CSR.Game.GameObject;
using ProtoBuf;

namespace CSR.DataTransmission;

[ProtoContract]
public class Packet_DepartStart
{
	[ProtoMember(1)]
	public List<GamePlayer> PlayerList { get; set; }
	[ProtoMember(2)]
	public List<CardEffectModule.Result> AttackResults { get; set; }
}