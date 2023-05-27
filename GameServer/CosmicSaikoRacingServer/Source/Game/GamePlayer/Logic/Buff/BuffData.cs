using CSR.Game.GameObject;
using ProtoBuf;

namespace CSR.Game.Player;


[ProtoContract]
public class BuffData
{
	[ProtoMember(1)]
	public List<Buff> List { get; set; }
}