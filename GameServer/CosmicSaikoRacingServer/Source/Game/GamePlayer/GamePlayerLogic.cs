using CSR.Game.GameObject;
using CSR.Game.Phase;
using EdenNetwork;

namespace CSR.Game;

public partial class GamePlayer
{
        public const int INITIAL_TARGET_DISTANCE = 100;
        //페이즈
        // public GameScene scene;
        
        public GamePlayer(PeerId clientId, int index, string nickname, List<GamePlayer> parent, PreheatPhase preheatPhase)
        {
            this.clientId = clientId;
            Index = index;
            Nickname = nickname;
            Exp = 0;
            Level = 1;
            
            this.parent = parent;
            this.Rank = 1;

            Buff = new BuffData();
            this.InitBuff(preheatPhase);
            Resource = new ResourceData();
            this.InitResource();
            Card = new CardData();
            this.InitCard();
            Depart = new DepartData();
            this.InitDepart();
            Maintain = new MaintainData();
            this.InitMaintain();

            
            RemainDistance = INITIAL_TARGET_DISTANCE;
            CurrentDistance = 0;
            TurnDistance = 0;

            PhaseReady = false;
            
            //임시 초기화
            int[,] card = {
                {1,1,2,2,2,3,3,11,11,11}, // 노말
                {26,26,46,46,66,66,86,86,106,106}, // 진영 기본
                {16,16,22,22,26,26,28,28,29,29}, // 화석
                {37,37,43,43,46,46,46,48,48,48}, // 전기
                {56,56,57,57,62,62,66,66,67,67}, // 핵
                {76,76,77,77,81,81,86,86,88,88}, // 바이오
                {96,96,97,97,102,102,106,106,109,109} // 코스믹
            };
            Random random = new Random();
            int randomNumber = random.Next(7);
            for (int i = 0; i < 10; i++)
            {
                this.AddCardToDeck(CardManager.GetCard(card[randomNumber,i]));
            }
        }

        /// <summary>
        /// 모니터링 플레이어 데이터는 덱정보 빼고 전달
        /// </summary>
        public GamePlayer CloneForMonitor()
        {
            GamePlayer hidePlayer = (GamePlayer)this.MemberwiseClone();
            hidePlayer.Card = new CardData();
            return hidePlayer;
        }
        
        /// <summary>
        /// 예열 페이즈가 실행할때 플레이어 정보를 설정한다
        /// </summary>
        public void PreheatStart()
        {
            PhaseReady = false;

            this.CardOnTurnStart();
            this.ResourceOnTurnStart();
            
            this.BuffOnPreheatStart();
        }
        
        /// <summary>
        /// 예열 페이즈가 끝날때 플레이어 정보를 설정한다
        /// </summary>
        public void PreheatEnd()
        {
            this.CardOnTurnEnd();
            this.BuffOnPreheatEnd();
        }

        /// <summary>
        /// 발진 페이즈 시작시 공격 후 이동거리를 늘린다
        /// </summary>
        public void DepartStart(out CardEffect.Result attackResult)
        {
            PhaseReady = false;
            //버프 적용
            this.BuffOnDepartStart();

            this.DepartOnTurnStart(out attackResult);

            
            CurrentDistance += TurnDistance;
            RemainDistance -= TurnDistance;
            TurnDistance = 0;
        }

        /// <summary>
        /// 정비 페이즈 시작
        /// </summary>
        public void MaintainStart()
        {
            PhaseReady = false;
            this.MaintainOnTurnStart();
        }
        
        /// <summary>
        /// 정비 페이즈 종료
        /// </summary>
        public void MaintainEnd()
        {
            this.MaintainOnTurnEnd();
        }
}