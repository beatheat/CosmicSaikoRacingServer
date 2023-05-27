using EdenNetwork;
using ProtoBuf;

namespace CSR.Game.Player;

[ProtoContract]
public partial class GamePlayer
{
	//플레이어의 고유식별자
	public PeerId ClientId { get; set; }

	//플레이어가 속한 리스트
	public List<GamePlayer> parent;

	//리스트에서의 인덱스
	[ProtoMember(1)] public int Index { get; set; }

	//플레이어의 닉네임
	[ProtoMember(2)] public string Nickname { get; set; }

	//남은 이동거리
	[ProtoMember(3)] public int RemainDistance { get; set; }

	//현재 이동거리
	[ProtoMember(4)] public int CurrentDistance { get; set; }

	//이번턴에 이동할 거리
	[ProtoMember(5)] public int TurnDistance { get; set; }

	//현재 등수 (이동거리가 크면 1등)
	[ProtoMember(6)] public int Rank { get; set; }
	
	//한 페이즈에서의 레디여부
	[ProtoMember(7)] public bool PhaseReady { get; set; }

	//경험치
	[ProtoMember(8)] public int Exp { get; set; }

	//레벨
	[ProtoMember(9)] public int Level { get; set; }

	[ProtoMember(10)] public CardData Card { get; set; }

	[ProtoMember(11)] public ResourceData Resource { get; set; }

	[ProtoMember(12)] public BuffData Buff { get; set; }

	[ProtoMember(13)] public DepartData Depart { get; set; }

	[ProtoMember(14)] public MaintainData Maintain { get; set; }
	
}