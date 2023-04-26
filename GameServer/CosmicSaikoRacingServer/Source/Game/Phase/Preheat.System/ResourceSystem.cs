using System.Text.Json.Serialization;

namespace CSRServer.Game;

public class ResourceSystem
{
	//최초 리롤 카운트
	private const int INITIAL_REROLL_COUNT = 4;
	//최초 리소스릴 카운트
	private const int INITIAL_REEL_COUNT = 4;
	
	//리소스릴
	private List<Resource.Type> _reel;
	//리소스릴의 리롤 카운트
	private int _rerollCount;
	//턴 시작 시 부여받는 리롤 카운트
	private int _availableRerollCount;
	//리소스 릴 카운트
	private int _reelCount;
	
	private readonly GamePlayer _player;
	
	public List<Resource.Type> reel => _reel;
	public int rerollCount => _rerollCount;
	public int reelCount => _reelCount;


	public ResourceSystem(GamePlayer player)
	{
		this._player = player;
		
		_reel = new List<Resource.Type>();
		_availableRerollCount = INITIAL_REROLL_COUNT;
		_rerollCount = _availableRerollCount;
		_reelCount = INITIAL_REEL_COUNT;
	}
	
	/// <summary>
	/// 리소스릴 리롤 카운트 증가
	/// </summary>
	public void AddRerollCount(int count)
	{
		_rerollCount += count;
	}

	/// <summary>
	/// 예열턴 시작 리소스릴 리롤 카운트 증가 
	/// </summary>
	public void AddAvailableRerollCount(int count)
	{
		_availableRerollCount += count;
	}
	
	/// <summary>
	/// 리소스릴 갯수 증가
	/// </summary>
	public void AddReelCount(int count)
	{
		_reelCount += count;
	}

	/// <summary>
	/// 예열턴 시작 로직
	/// </summary>
	public void TurnStart()
	{
		_rerollCount = _availableRerollCount;
		RollResource();
	}
	
	/// <summary>
	/// 리소스릴에 있는 리소스를 랜덤으로 다시 뽑는다
	/// </summary>
	private List<Resource.Type> RollResource(List<int>? resourceFixed = null)
	{
		for (int i = 0; i < _reelCount; i++)
		{
			Resource.Type resource = Util.GetRandomEnumValue<Resource.Type>();
			//리소스릴 개수가 늘어났다면 리소스릴 새로 추가
			if (i >= _reel.Count)
				_reel.Add(resource);
			//리소스릴 개수가 그대로라면 기존에 있던 릴에 덮어쓴다
			else if (resourceFixed == null || !resourceFixed.Contains(i))
				_reel[i] = resource;
		}
            

		return _reel;
	}

	/// <summary>
	/// 최초로 받은 리소스를 이후 다시 배정받는다
	/// </summary>
	public List<Resource.Type>? RerollResource(List<int>? resourceFixed = null)
	{
		//리롤카운트가 0보다 클때만 사용할 수 있고 사용할때마다 1씩 줄어든다
		if (_rerollCount > 0)
		{
			_rerollCount--;
			//롤 리소스전 버프 적용
			_player.buffSystem.BeforeRerollResource(ref resourceFixed);

			RollResource(resourceFixed);
			//롤 리소스후 버프 적용
			_player.buffSystem.AfterRerollResource(ref resourceFixed, ref _reel);
			return _reel;
		}
		return null;
	}

	/// <summary>
	/// 카드 사용 조건 검사한다
	/// </summary>
	public bool CheckCardUsable(Card card)
	{
		return card.condition.Check(_reel);
	}
}