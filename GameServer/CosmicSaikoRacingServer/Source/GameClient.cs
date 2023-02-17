using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSRServer
{
    /// <summary>
    /// 서버에 접속한 클라이언트를 관리하는 클래스
    /// </summary>
    public class GameClient
    {
        public string id;
        public bool isConnected;

        public GameClient(string id)
        {
            this.id = id;
            this.isConnected = true;
        }
    }
}
