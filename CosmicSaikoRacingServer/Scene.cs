using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdenNetwork;

namespace EdenServer
{
    public abstract class Scene
    {
        protected GameManager gameManager;
        protected EdenNetServer server;

        public Dictionary<string, object> passingData = new Dictionary<string, object>();

        public Scene(GameManager gameManager, EdenNetServer server)
        {
            this.gameManager = gameManager;
            this.server = server;
        }

        public virtual void Load() { }
        public virtual void Destroy() { }
    }
}
