using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSRServer
{
    public class GameClient
    {
        string id;
        public bool isConnected;

        public GameClient(string id)
        {
            this.id = id;
            this.isConnected = true;
        }
    }
}
