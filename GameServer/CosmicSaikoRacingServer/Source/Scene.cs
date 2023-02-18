﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdenNetwork;

namespace CSRServer
{
    /// <summary>
    /// 게임 화면의 최소단위인 씬을 구성하는 클래스
    /// </summary>
    public abstract class Scene
    {
        protected GameManager gameManager;
        protected EdenNetServer server;

        public Dictionary<string, object> passingData = new Dictionary<string, object>();
        public Dictionary<string, object>? passedData = null;

        public Scene(GameManager gameManager, EdenNetServer server)
        {
            this.gameManager = gameManager;
            this.server = server;
        }

        public virtual void Load() { }
        public virtual void Destroy() { }
    }
}