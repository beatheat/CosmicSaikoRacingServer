using System.Text.Json.Serialization;

namespace CSRServer.Lobby
{
    /// <summary>
    /// 로비씬에서 하나의 클라이언트를 표현하는 클래스
    /// </summary>
    public class LobbyPlayer
    {
        [JsonIgnore]
        public string clientId;

        public int id;
        public string nickname;
        public bool isReady;
        public bool host;

        public LobbyPlayer(string clientId, string nickname, int index, bool host = false)
        {
            this.clientId = clientId;
            this.id = clientId.GetHashCode();
            this.nickname = nickname;
            this.isReady = false;
            this.host = host;
        }
    }
}
