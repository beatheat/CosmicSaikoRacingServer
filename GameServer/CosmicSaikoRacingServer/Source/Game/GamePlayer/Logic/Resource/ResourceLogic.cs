using CSR.Game.GameObject;

namespace CSR.Game;

public static class ResourceLogic
{
	//최초 리롤 카운트
	private const int INITIAL_REROLL_COUNT = 4;
	//최초 리소스릴 카운트
	private const int INITIAL_REEL_COUNT = 4;


	public static void InitResource(this GamePlayer player)
	{
		player.Resource.Reel = new List<ResourceType>();
		player.Resource.AvailableRerollCount = INITIAL_REROLL_COUNT;
		player.Resource.RerollCount = INITIAL_REROLL_COUNT;
		player.Resource.ReelCount = INITIAL_REEL_COUNT;
	}
	
	/// <summary>
	/// 리소스릴 리롤 카운트 증가
	/// </summary>
	public static void AddRerollCount(this GamePlayer player, int count)
	{
		player.Resource.RerollCount += count;
	}

	/// <summary>
	/// 예열턴 시작 리소스릴 리롤 카운트 증가 
	/// </summary>
	public static void AddAvailableRerollCount(this GamePlayer player, int count)
	{
		player.Resource.AvailableRerollCount += count;
	}
	
	/// <summary>
	/// 리소스릴 갯수 증가
	/// </summary>
	public static void AddReelCount(this GamePlayer player, int count)
	{
		player.Resource.ReelCount += count;
	}

	/// <summary>
	/// 예열턴 시작 로직
	/// </summary>
	public static void ResourceOnTurnStart(this GamePlayer player)
	{
		player.Resource.RerollCount = player.Resource.AvailableRerollCount;
		RollResource(player);
	}
	
	/// <summary>
	/// 리소스릴에 있는 리소스를 랜덤으로 다시 뽑는다
	/// </summary>
	private static void RollResource(this GamePlayer player, List<int>? resourceFixed = null)
	{
		for (int i = 0; i < player.Resource.ReelCount; i++)
		{
			ResourceType resource = Util.GetRandomEnumValue<ResourceType>();
			//리소스릴 개수가 늘어났다면 리소스릴 새로 추가
			if (i >= player.Resource.Reel.Count)
				player.Resource.Reel.Add(resource);
			//리소스릴 개수가 그대로라면 기존에 있던 릴에 덮어쓴다
			else if (resourceFixed == null || !resourceFixed.Contains(i))
				player.Resource.Reel[i] = resource;
		}
	}

	/// <summary>
	/// 최초로 받은 리소스를 이후 다시 배정받는다
	/// </summary>
	public static List<ResourceType>? RerollResource(this GamePlayer player, List<int>? resourceFixed = null)
	{
		//리롤카운트가 0보다 클때만 사용할 수 있고 사용할때마다 1씩 줄어든다
		if (player.Resource.RerollCount > 0)
		{
			player.Resource.RerollCount--;
			//롤 리소스전 버프 적용
			player.BuffBeforeRerollResource(ref resourceFixed);

			RollResource(player, resourceFixed);
			//롤 리소스후 버프 적용
			player.BuffAfterRerollResource(resourceFixed);
			return player.Resource.Reel;
		}
		return null;
	}

	/// <summary>
	/// 카드 사용 조건 검사한다
	/// </summary>
	public static bool CheckCardUsable(this GamePlayer player, Card card)
	{
		return card.Condition.Check(player.Resource.Reel);
	}
}