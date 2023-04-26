using System.Text.Json.Serialization;


namespace CSRServer.Game
{
    [JsonConverter(typeof(GamePlayerJsonConverter))]
    public class GamePlayer
    {
        //목표 이동거리
        [JsonIgnore] 
        public const int INITIAL_TARGET_DISTANCE = 100;

        //플레이어의 고유식별자
        [JsonIgnore]
        public string clientId  { get; }
        //플레이어가 속한 리스트
        [JsonIgnore]
        public List<GamePlayer> parent {  get; }

        //리스트에서의 인덱스
        public int index { get; }

        //플레이어의 닉네임
        public string nickname {  get; }
        
        //남은 이동거리
        public int remainDistance { set; get; }
        //현재 이동거리
        public int currentDistance { set; get; }
        //이번턴에 이동할 거리
        public int turnDistance { set; get; }
        
        //현재 등수 (이동거리가 크면 1등)
        public int rank { set; get; }
        
        //한 페이즈에서의 레디여부
        [JsonIgnore]
        public bool phaseReady { set; get; }

        //카드 시스템
        public CardSystem cardSystem { get; }
        //리소스 시스템
        public ResourceSystem resourceSystem { get; }
        //버프 시스템
        public BuffSystem buffSystem { get; }
        //발진 시스템
        public DepartSystem departSystem { get; }
        //정비 시스템
        public MaintainSystem maintainSystem { get; }

        //페이즈
        [JsonIgnore]
        public GameScene scene;
        
        public GamePlayer(string clientId, int index, string nickname, List<GamePlayer> parent, GameScene scene)
        {
            this.clientId = clientId;
            this.index = index;
            this.nickname = nickname;

            this.parent = parent;
            this.scene = scene;
            this.rank = 1;

            buffSystem = new BuffSystem(this);
            resourceSystem = new ResourceSystem(this);
            cardSystem = new CardSystem(this);
            
            departSystem = new DepartSystem();
            
            maintainSystem = new MaintainSystem(this);
            
            // artifactList = new List<Artifact>();
            
            remainDistance = INITIAL_TARGET_DISTANCE;
            currentDistance = 0;
            turnDistance = 0;

            phaseReady = false;
            
            //임시 초기화
            int[,] card = {
                {0,0,1,1,3,3,4,4,6,6},
                {23,23,52,52,80,80,110,110,141,141},
                {21,21,25,25,26,26,30,30,32,32},
                {51,51,52,52,53,53,54,54,55,55},
                {52,52,53,53,54,54,56,56,57,57},
                // {80,80,84,84,85,85,87,87,92,92},
                {110,110,110,115,115,115,115,121,121,121},
                {140,140,144,144,146,146,157,157,151,151}
            };
            Random random = new Random();
            int randomNumber = random.Next(7);
            for (int i = 0; i < 10; i++)
            {
                cardSystem.AddCardToDeck(CardManager.GetCard(card[randomNumber,i]));
            }
        }

        /// <summary>
        /// 모니터링 플레이어 데이터는 덱정보 빼고 전달
        /// </summary>
        public GamePlayer CloneForMonitor()
        {
            GamePlayer hidePlayer = (GamePlayer)this.MemberwiseClone();
            // hidePlayer.deck = null!;
            // hidePlayer.usedCard = null!;
            // hidePlayer.unusedCard = null!;
            return hidePlayer;
        }
        
        /// <summary>
        /// 예열 페이즈가 실행할때 플레이어 정보를 설정한다
        /// </summary>
        public void PreheatStart()
        {
            phaseReady = false;

            resourceSystem.TurnStart();
            cardSystem.TurnStart();
            
            buffSystem.OnPreheatStart();
        }
        
        /// <summary>
        /// 예열 페이즈가 끝날때 플레이어 정보를 설정한다
        /// </summary>
        public void PreheatEnd()
        {
            cardSystem.TurnEnd();
            buffSystem.OnPreheatEnd();
        }

        /// <summary>
        /// 발진 페이즈 시작시 공격 후 이동거리를 늘린다
        /// </summary>
        public void DepartStart(out CardEffectModule.Result[] attackResult)
        {
            phaseReady = false;
            //버프 적용
            buffSystem.OnDepartStart();
            
            departSystem.TurnStart(out attackResult);

            currentDistance += turnDistance;
            remainDistance -= turnDistance;
            turnDistance = 0;
        }

        /// <summary>
        /// 정비 페이즈 시작
        /// </summary>
        public void MaintainStart()
        {
            phaseReady = false;
            maintainSystem.TurnStart();
        }
        
        /// <summary>
        /// 정비 페이즈 종료
        /// </summary>
        public void MaintainEnd()
        {
            maintainSystem.TurnEnd();
        }
    }
}
