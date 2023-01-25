using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CSRServer
{
    internal class ModuleChip
    {
        public int id;

        [JsonIgnore]
        public int rank;
        [JsonIgnore]
        public Resource.Type type;
        //public string name;
        //public string description;
    }
}
