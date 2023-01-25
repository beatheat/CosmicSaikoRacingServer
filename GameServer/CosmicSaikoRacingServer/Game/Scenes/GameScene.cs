using EdenNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;


namespace CSRServer
{
    internal class GameScene : Scene
    {
        public GameScene(GameManager gameManager, EdenNetServer server) : base(gameManager, server)
        {
        }
    }
}
