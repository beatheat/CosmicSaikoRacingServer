using CSR.DataTransmission;
using CSR.Game.GameObject;
using CSR.Game.Player;
using EdenNetwork;

namespace CSR.Game.Phase;

public class PreheatPhase
{
    private const int INITIAL_TIME = 99;
    
    private readonly EdenUdpServer _server;
    private readonly GameSession _parent;

    private Timer? _timer;
    private int _time;
    private bool _turnEnd;
    
    public PreheatPhase(GameSession parent, EdenUdpServer server)
    {
        _server = server;
        _parent = parent;
        _timer = null;
        _time = 0;
        _turnEnd = false;
    }

    /// <summary>
    /// 예열 페이즈 시작
    /// </summary>
    public void PreheatStart()
    {

        _time = INITIAL_TIME;
        _turnEnd = false;

        _server.AddEndpoints(this);
        
        foreach (var player in _parent.PlayerList)
        {
            player.PreheatStart();
        }

        var orderedPlayerList = _parent.PlayerList.OrderByDescending(p => p.CurrentDistance).ToList();
        //플레이어 rank 정해주기
        for (int i = 0; i < orderedPlayerList.Count; i++)
            orderedPlayerList[i].Rank = i + 1;

        // 예열 페이즈 시작시 턴 데이터 클라이언트와 동기화
        var monitorPlayerList = _parent.GetMonitorPlayerList();
        foreach (var player in _parent.PlayerList)
        {
            _server.Send("PreheatStart", player.ClientId, new Packet_PreheatStart
            {
                PlayerList = monitorPlayerList,
                Player = player,
                Turn = _parent.Turn,
                Timer = _time // TODO: 삭제고려
            });
        }

        _timer = new Timer(GameTimer, null, 0, 1000);
    }

    /// <summary>
    /// 예열 페이즈 준비완료
    /// </summary>
    public void Ready(GamePlayer player)
    {
        player.PhaseReady = true;

        //모든 플레이어가 예열페이즈를 마쳤는지 체크
        bool checkAllReady = true;
        foreach (var p in _parent.PlayerList)
            checkAllReady = checkAllReady && p.PhaseReady;
        //모든 플레이어가 예열페이즈를 마쳤다면 예열페이즈 종료
        if (checkAllReady)
            PreheatEnd();
    }

    /// <summary>
    /// 예열 페이즈 종료
    /// </summary>
    private void PreheatEnd()
    {
        lock (this)
        {
            if (_turnEnd == false)
            {
                _timer?.Dispose();
                _server.RemoveEndpoints(this);
                foreach (var player in _parent.PlayerList)
                    player.PreheatEnd();
                _parent.DepartStart();
                _turnEnd = true;
            }
        }
    }

    /// <summary>
    /// 예열 페이즈 타이머
    /// </summary>
    private async void GameTimer(object? sender)
    {
        if (_time > 0)
        {
            _time--;
            await _server.BroadcastAsync("PreheatTime", _time);
        }
        else
        {
            PreheatEnd();
        }
    }

    #region Receive/Response Methods

    /// <summary>
    /// 예열턴 준비완료 API 
    /// TurnEnd 효과 때문에 public으로 선언
    /// </summary>
    [EdenReceive]
    private void PreheatReady(PeerId clientId)
    {
        var player = _parent.PlayerList.Find(player => player.ClientId == clientId);
        if (player == null)
            return;
        Ready(player);
    }

    /// <summary>
    /// 카드 사용 API
    /// </summary>
    [EdenResponse]
    private Response_UseCard UseCard(PeerId clientId, Request_UseCard request)
    {
        var player = _parent.PlayerList.Find(player => player.ClientId == clientId);
        if (player == null)
            return new Response_UseCard{ErrorCode = ErrorCode.PlayerNotExist};

        if (player.PhaseReady)
            return new Response_UseCard{ErrorCode = ErrorCode.PlayerTurnEnd};

        if (!player.ValidateHandIndex(request.Index))
            return new Response_UseCard{ErrorCode = ErrorCode.UseCard_WrongIndex};
        
        if (!player.IsCardEnable(request.Index))
            return new Response_UseCard{ErrorCode = ErrorCode.UseCard_ResourceConditionNotSatisfy};

        if (!player.UseCard(request.Index, this, out var results))
            return new Response_UseCard{ErrorCode = ErrorCode.UseCard_CardRestrictedByBuff};
        
        return new Response_UseCard {Player = player, Results = results};
    }

    /// <summary>
    /// 리롤 리소스 API
    /// </summary>
    [EdenResponse]
    private Response_RerollResource RerollResource(PeerId clientId, Request_RerollResource request)
    {
        var player = _parent.PlayerList.Find(player => player.ClientId == clientId);
        
        if (player == null)
            return new Response_RerollResource {ErrorCode = ErrorCode.PlayerNotExist};
        
        if (player.PhaseReady)
            return new Response_RerollResource{ErrorCode = ErrorCode.PlayerTurnEnd};

        if (request.ResourceFixed == null)
            return new Response_RerollResource {ErrorCode = ErrorCode.MissingPacketData};
        
        var result = player.RerollResource(request.ResourceFixed);
        if (result == null)
            return new Response_RerollResource {ErrorCode = ErrorCode.RerollResource_RerollCountZero};
        
        return new Response_RerollResource {Player = player};
    }

    #endregion
}