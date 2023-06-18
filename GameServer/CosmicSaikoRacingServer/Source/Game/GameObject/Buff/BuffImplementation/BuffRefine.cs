using CSR.Game.Phase;
using ProtoBuf;

namespace CSR.Game.GameObject;


[ProtoContract]
internal class BuffRefine : Buff
{
	private const int REFINE_COEFFICIENT = 5;

	public BuffRefine(PreheatPhase phase, GamePlayer player) : base(phase, player)
	{
		Type = BuffType.Refine;
	}

	/// <summary>
	/// 리롤후 정제버프가 존재한다면 적용한다
	/// </summary>
	public override void AfterRerollResource(List<int>? resourceFixed)
	{
		if (Count == 0) return;
		int cardTypeCount = Enum.GetValues(typeof(CardType)).Length;
		int[] frequentList = Enumerable.Repeat<int>(0, cardTypeCount).ToArray();

		//현재 패에서 가장 많은 카드 타입을 찾는다
		foreach (var card in player.Card.Hand)
		{
			frequentList[(int) card.Type]++;
		}

		int mostFqCount = 0;
		int mostFqCardType = 0;
		for (int i = 0; i < cardTypeCount; i++)
		{
			if (mostFqCount > frequentList[i])
			{
				mostFqCount = frequentList[i];
				mostFqCardType = i;
			}
		}

		if (mostFqCardType == (int) CardType.Normal)
			return;

		//가장 많은 타입이 리롤시 가장 등장할 확률이 높다

		int resourceTypeCount = Enum.GetValues((typeof(ResourceType))).Length;
		double highPercentage = 100.0 / Resource.COUNT + REFINE_COEFFICIENT * Count;
		double[] resourcePercentage = Enumerable.Repeat((100.0 - highPercentage) / (resourceTypeCount - 1), Resource.COUNT).ToArray();
		resourcePercentage[mostFqCardType] = highPercentage;

		for (int i = 0; i < player.Resource.ReelCount; i++)
		{
			var resource = Util.GetRandomEnumValue<ResourceType>(resourcePercentage);
			if (i >= player.Resource.Reel.Count)
				player.Resource.Reel.Add(resource);
			else if (resourceFixed == null || !resourceFixed.Contains(i))
				player.Resource.Reel[i] = resource;
		}
	}
}