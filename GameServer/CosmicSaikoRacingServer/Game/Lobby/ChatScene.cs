using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdenNetwork;

namespace CSRServer
{
    public class ChatScene : Scene
    {

        public ChatScene(GameManager gameManager, EdenNetServer server) : base(gameManager, server)
        {

        }

        public override void Load()
        {
            server.AddReceiveEvent("sChat", sChat);
        }

        public override void Destroy()
        {
            server.RemoveReceiveEvent("sChat");
        }

        public void sChat(string clientId, EdenData data)
        {
            server.Broadcast("cChat", data);
        }
    }
}
