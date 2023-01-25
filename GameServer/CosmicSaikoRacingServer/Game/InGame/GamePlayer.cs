using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CSRServer
{
    internal class GamePlayer
    {
        [JsonIgnore]
        public string clientId;

        public int id;
        public string nickname;

        List<ModuleChip> deck;
        List<ModuleChip> usedChips;
        List<ModuleChip> unusedChips;

        List<Resource> resources;

        List<ModuleChip> hand;
        List<Artifact> artifact;
    }
}
