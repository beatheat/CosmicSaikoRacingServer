using CSR.DataTransmission;
using CSR.Game.GameObject;
using EdenNetwork;

namespace CSR.Game.Phase;

public class MaintainPhase
{
    private const int INITIAL_TIME = 99;
    private readonly GameSession _parent;
    private readonly IEdenNetServer _server;

    private Timer? _timer;
    private int _time;
    private bool _turnEnd;

    private object _turnEndLock;
    
    public MaintainPhase(GameSession parent, IEdenNetServer server)
    {
        this._server = server;
        this._parent = parent;
        this._timer = null;
        this._time = 0;
        this._turnEnd = false;
        this._turnEndLock = new object();
    }
    

    /// <summary>
    /// 정비 페이즈 시작
    /// </summary>
    public void MaintainStart()
    {
        _server.AddEndpoints(this);

        _time = INITIAL_TIME;
        _turnEnd = false;   
        //정비 페이즈 구성요소 클라이언트와 동기화
        foreach (var player in _parent.PlayerList)
        {
            player.MaintainStart();
            _server.Send("MaintainStart", player.clientId, new Packet_MaintainStart
            {
                ShopCards = player.Maintain.ShopCards,
                RemoveCards = player.Maintain.RemoveCards
            });
        }

        // _timer = new Timer(GameTimer, null, 0, 1000);
    }

    /// <summary>
    /// 정비 페이즈 종료
    /// </summary>
    private void MaintainEnd()
    {
        lock (_turnEndLock)
        {
            if (_turnEnd == false)
            {
                _timer?.Dispose();

                _server.RemoveEndpoints(this);

                foreach (var player in _parent.PlayerList)
                    player.MaintainEnd();

                _parent.PreheatStart();
                _turnEnd = true;
            }
        }
    }

    // 정비 페이즈 타이머
    private async void GameTimer(object? sender)
    {
        if (_time > 0)
        {
            _time--;
            await _server.BroadcastAsync("MaintainTime", _time);
        }
        else
        {
            MaintainEnd();
        }
    }

    #region Receive/Response Methods

    /// <summary>
    /// 정비 페이즈 준비완료 API
    /// </summary>
    [EdenReceive]
    private void MaintainReady(PeerId clientId)
    {
        var player = _parent.PlayerList.Find(player => player.clientId == clientId);
        if (player == null)
            return;
        
        player.PhaseReady = true;

        //모든 플레이어가 정비턴을 마쳤는지 체크
        bool checkAllReady = true;
        foreach (var p in _parent.PlayerList)
            checkAllReady = checkAllReady && p.PhaseReady;
        if (checkAllReady)
            MaintainEnd();
    }


    /// <summary>r
    /// 구매카드 상점 리롤 API
    /// </summary>
    [EdenResponse]
    private Response_RerollShop RerollShop(PeerId clientId)
    {
        var player = _parent.PlayerList.Find(player => player.clientId == clientId);
        if (player == null)
            return new Response_RerollShop {ErrorCode = ErrorCode.PlayerNotExist};

        if (player.PhaseReady)
            return new Response_RerollShop {ErrorCode = ErrorCode.PlayerNotExist};

        
        if (player.RerollShop() == MaintainLogic.ErrorCode.COIN_NOT_ENOUGH)
            return new Response_RerollShop {ErrorCode = ErrorCode.MaintainPhase_CoinNotEnough};
        
        return new Response_RerollShop {Coin = player.Maintain.Coin, ShopCards = player.Maintain.ShopCards};
    }

    /// <summary>
    /// 제거카드 상점 리롤 API
    /// </summary>
    [EdenResponse]
    private Response_RerollRemoveCard RerollRemoveCard(PeerId clientId)
    {

        var player = _parent.PlayerList.Find(player => player.clientId == clientId);
        if (player == null)
            return new Response_RerollRemoveCard {ErrorCode = ErrorCode.PlayerNotExist};

        if (player.PhaseReady)
            return new Response_RerollRemoveCard {ErrorCode = ErrorCode.PlayerNotExist};

        if (player.RerollRemoveCard() == MaintainLogic.ErrorCode.COIN_NOT_ENOUGH)
            return new Response_RerollRemoveCard {ErrorCode = ErrorCode.MaintainPhase_CoinNotEnough};

        return new Response_RerollRemoveCard {Coin = player.Maintain.Coin, RemoveCards = player.Maintain.RemoveCards};
    }

    /// <summary>
    /// 경험치 구매 API
    /// </summary>
    [EdenResponse]
    private Response_BuyExp BuyExp(PeerId clientId)
    {
        var player = _parent.PlayerList.Find(player => player.clientId == clientId);
        if (player == null)
            return new Response_BuyExp {ErrorCode = ErrorCode.PlayerNotExist};

        if (player.PhaseReady)
            return new Response_BuyExp {ErrorCode = ErrorCode.PlayerNotExist};
        
        var errorCode = player.BuyExp();

        if (errorCode == MaintainLogic.ErrorCode.COIN_NOT_ENOUGH)
            return new Response_BuyExp {ErrorCode = ErrorCode.MaintainPhase_CoinNotEnough};
        else if (errorCode == MaintainLogic.ErrorCode.MAX_LEVEL)
            return new Response_BuyExp {ErrorCode = ErrorCode.MaintainPhase_MaxLevel};

        return new Response_BuyExp {Coin = player.Maintain.Coin, Level = player.Level, Exp = player.Exp};
    }

    /// <summary>
    /// 구매카드 상점 구매 API
    /// </summary>
    [EdenResponse]
    private Response_BuyCard BuyCard(PeerId clientId, Request_BuyCard request)
    {
        var player = _parent.PlayerList.Find(player => player.clientId == clientId);
        if (player == null)
            return new Response_BuyCard {ErrorCode = ErrorCode.PlayerNotExist};

        if (player.PhaseReady)
            return new Response_BuyCard {ErrorCode = ErrorCode.PlayerNotExist};
        
        var errorCode = player.BuyCard(request.Index, out var buyCard);
        
        if (errorCode == MaintainLogic.ErrorCode.COIN_NOT_ENOUGH)
            return new Response_BuyCard {ErrorCode = ErrorCode.MaintainPhase_CoinNotEnough};
        else if (errorCode == MaintainLogic.ErrorCode.WRONG_INDEX)
            return new Response_BuyCard {ErrorCode = ErrorCode.BuyCard_WrongIndex};
        
        player.AddCardToDeck(buyCard);

        return new Response_BuyCard {BuyCard = buyCard, ShopCards = player.Maintain.ShopCards, Coin = player.Maintain.Coin, Deck = player.Card.Deck};
    }

    /// <summary>
    /// 제커 카드 상점 제거 API
    /// </summary>
    [EdenResponse]
    private Response_RemoveCard RemoveCard(PeerId clientId, Request_RemoveCard request)
    {
        var player = _parent.PlayerList.Find(player => player.clientId == clientId);
        if (player == null)
            return new Response_RemoveCard {ErrorCode = ErrorCode.PlayerNotExist};

        if (player.PhaseReady)
            return new Response_RemoveCard {ErrorCode = ErrorCode.PlayerNotExist};
        

        var errorCode = player.RemoveCard(request.Index, out var removeCard);

        if (errorCode == MaintainLogic.ErrorCode.COIN_NOT_ENOUGH)
            return new Response_RemoveCard {ErrorCode = ErrorCode.MaintainPhase_CoinNotEnough};

        else if (errorCode == MaintainLogic.ErrorCode.WRONG_INDEX)
            return new Response_RemoveCard {ErrorCode = ErrorCode.RemoveCard_WrongIndex};

        return new Response_RemoveCard {Coin = player.Maintain.Coin, Deck = player.Card.Deck, RemoveCards = player.Maintain.RemoveCards};
    }

    #endregion

}