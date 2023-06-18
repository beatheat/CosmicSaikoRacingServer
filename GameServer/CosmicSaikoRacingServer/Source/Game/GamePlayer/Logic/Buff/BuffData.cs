using CSR.Game.GameObject;
using ProtoBuf;

#pragma warning disable CS8618

namespace CSR.Game;

[ProtoContract]
public class BuffData
{
	[ProtoMember(1)]
	public List<Buff> List { get; set; }
}