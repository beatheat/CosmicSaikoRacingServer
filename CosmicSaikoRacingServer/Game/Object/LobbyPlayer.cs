using System.Text.Json.Serialization;

namespace CSRServer
{
    public class LobbyPlayer
    {
        [JsonIgnore]
        public string clientId;

        public int id;
        public string nickname;
        public bool isReady;

        public LobbyPlayer(string clientId, string nickname, int index)
        {
            this.clientId = clientId;
            this.id = clientId.GetHashCode();
            this.nickname = nickname;
            this.isReady = false;
        }
    }
}
