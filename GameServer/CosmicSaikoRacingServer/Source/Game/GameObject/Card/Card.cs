using ProtoBuf;

#pragma warning disable CS8618

namespace CSR.Game.GameObject;

[ProtoContract]
public partial class Card
{
    //카드의 식별자
    [ProtoMember(1)]
    public int Id { get; set; }

    [ProtoMember(2)]
    public Dictionary<string, int> Variables => variables.ToDictionary(x => x.Key, x => x.Value.value);
    
    //카드 리소스 조건
    [ProtoMember(3)]
    public CardCondition Condition { get; set; }

    //피폭 버프
    [ProtoMember(4)]
    public bool IsExposure { get; set; } = false;

    //의태 버프
    [ProtoMember(5)]
    public bool IsMimesis { get; set; } = false;

    //카드 소멸
    [ProtoMember(6)]
    public bool Death { get; set; } = false;

    //카드 타입
    [ProtoMember(7)]
    public CardType Type { get; set; }

    //카드 등급
    [ProtoMember(8)]
    public int Rank { get; set; }
    
    //카드효과가 발생할 것인지 여부
    [ProtoMember(9)]
    public bool Enable { get; set; }= true;

    //카드 사용된 횟수
    [ProtoMember(10)]
    public int UsedCount { get; set; } = 0;
    
    //카드 변수
    public Dictionary<string, Variable> variables { get; set; }
    
    //카드 효과
    public CardEffect Effect { get; set; }
}
