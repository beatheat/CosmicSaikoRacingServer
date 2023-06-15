using EdenNetwork;

namespace MatchingServer
{
    internal class MatchingServer
    {
        public class MatchInfo
        {
            public PeerId id;
            public bool canceled = false;
        }

        public class LobbyRoom
        {
            public NatPeer host = null!;
            public DateTime createdTime;
        }
        
        public EdenUdpServer server;
        public Dictionary<int, LobbyRoom> lobbyRooms;
        public Queue<MatchInfo> matchQueue;
        public Dictionary<PeerId, MatchInfo> matchMap;

        private  bool closed = false;
        public MatchingServer(EdenUdpServer server)
        {
            this.server = server;
            lobbyRooms = new Dictionary<int, LobbyRoom>();
            matchQueue = new Queue<MatchInfo>();
            closed = false;
        }


        public void Run()
        {

            server.AddEndpoints(this);
            // Task.Run(async () =>
            // {
            //     while (true)
            //     {
            //         if (matchQueue.Count >= 4)
            //         {
            //             List<MatchInfo> matchGroup = new List<MatchInfo>();
            //             for (int i = 0; i < 4; i++)
            //             {
            //                 var clientInfo = matchQueue.Dequeue();
            //                 if (!clientInfo.canceled && server.StillConnected(clientInfo.id))
            //                     matchGroup.Add(clientInfo);
            //             }
            //             if (matchGroup.Count != 4)
            //             {
            //                 foreach (var clientInfo in matchGroup)
            //                     matchQueue.Enqueue(clientInfo);
            //                 continue;
            //             }
            //
            //             var hostAddress = StringToAddress(matchGroup[0].id);
            //             server.Send("StartHost", matchGroup[0].id);
            //             for (int i = 1; i < 4; i++)
            //             {
            //                 server.Send("StartClient", matchGroup[i].id, hostAddress);
            //             }
            //             for (int i = 0; i < 4; i++)
            //             {
            //                 if (matchMap.ContainsKey(matchGroup[i].id))
            //                     matchMap.Remove(matchGroup[i].id);
            //                 //server.DisconnectClient(matchGroup[i].id);
            //             }
            //         }
            //         await Task.Delay(100);
            //     }
            // });

            
            server.SetNatRequestListener();
            // server.SetNatRequestEvent(NatRequestEvent);
            server.Listen(9999);

            new Thread(CleanUnusedRooms).Start();
        }

        public void CleanUnusedRooms()
        {
            while (!closed)
            {
                var removeRoomNumbers = lobbyRooms.Where(item => DateTime.Now - item.Value.createdTime > TimeSpan.FromHours(1)).Select(item => item.Key).ToList();
                foreach (var roomNumber in removeRoomNumbers)
                    lobbyRooms.Remove(roomNumber);

                Thread.Sleep(10 * 1000);
            }
        }

        public void Close()
        {
            closed = true;
            server.Close();
        }

        //
        // [EdenResponse]
        // public NatPeer? GetLobbyAddress(PeerId clientId, int lobbyNumber)
        // {
        //     int roomNum;
        //     do
        //     {
        //         roomNum = (int)(DateTime.Now.Ticks % 100000L);
        //     } while (lobbyRooms.ContainsKey(roomNum));
        //     
        //     
        //     lobbyRooms.Add(roomNum, new LobbyRoom {host = new NatPeer(), createdTime = DateTime.Now});
        //
        //     if (lobbyRooms.TryGetValue(lobbyNumber, out var lobbyRoom))
        //         return lobbyRoom.host;
        //     return null;
        // }
        //
        //

        [EdenResponse]
        public int CreateLobby(PeerId clientId)
        {
            if (lobbyRooms.Count >= 100000)
            {
                // return EdenData.Error("There is no room remain");    
                return -1;
            }
            
            int lobbyNumer;
            do
            {
                lobbyNumer = (int)(DateTime.Now.Ticks % 100000L);
            } while (lobbyRooms.ContainsKey(lobbyNumer));
            
            
            lobbyRooms.Add(lobbyNumer, new LobbyRoom {host = new NatPeer(), createdTime = DateTime.Now});
            
            return lobbyNumer;
        }
        

        [EdenReceive]
        public void DestroyLobby(PeerId clientId, int lobbyNumber)
        {

            if (lobbyRooms.ContainsKey(lobbyNumber))
            {
                //게임 호스트만 삭제가능하다
                if (clientId.Ip == lobbyRooms[lobbyNumber].host.RemoteEndPoint.Ip)
                {
                    lobbyRooms.Remove(lobbyNumber);
                }
            }
        }

        [EdenNatRelay]
        private NatPeer? NatRequestEvent(NatPeer peer, string additionalData)
        {
            try
            {
                string type = additionalData.Split("/")[0];
                int lobbyNumber = int.Parse(additionalData.Split("/")[1]);
                
                Console.WriteLine($"{additionalData}");
                if (type == "host")
                {
                    if (lobbyRooms.ContainsKey(lobbyNumber))
                    {
                        lobbyRooms[lobbyNumber].host = peer;
                        Console.WriteLine($"Host Registered {peer.LocalEndPoint}/{peer.RemoteEndPoint}");
                    }
                }
                else if(type == "client")
                {
                    if (lobbyRooms.TryGetValue(lobbyNumber, out var hostNatPeer))
                        return hostNatPeer.host;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return null;
        }

        [EdenReceive]
        public void MatchMake(PeerId clientId)
        {
            MatchInfo matchInfo = new MatchInfo { id = clientId, canceled = false };
            matchQueue.Enqueue(matchInfo);
            matchMap.Add(matchInfo.id, matchInfo);
        }

        [EdenReceive]
        public void CancelMatchMake(PeerId clientId)
        {
            if(matchMap.ContainsKey(clientId))
            {
                matchMap[clientId].canceled = true;
            }
        }

    }
}
