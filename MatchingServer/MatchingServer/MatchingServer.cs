using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using EdenNetwork;
using System.Runtime.CompilerServices;

namespace MatchingServer
{
    internal class MatchingServer
    {
        public class MatchInfo
        {
            public PeerId id;
            public bool canceled = false;
        }

        public class Room
        {
            public NatPeer host = null!;
            public DateTime createdTime;
        }
        
        public EdenUdpServer server;
        public Dictionary<int, Room> rooms;
        public Queue<MatchInfo> matchQueue;
        public Dictionary<PeerId, MatchInfo> matchMap;

        private  bool closed = false;
        public MatchingServer(EdenUdpServer server)
        {
            this.server = server;
            rooms = new Dictionary<int, Room>();
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
                var removeRoomNumbers = rooms.Where(item => DateTime.Now - item.Value.createdTime > TimeSpan.FromHours(1)).Select(item => item.Key).ToList();
                foreach (var roomNumber in removeRoomNumbers)
                    rooms.Remove(roomNumber);

                Thread.Sleep(10 * 1000);
            }
        }

        public void Close()
        {
            closed = true;
            server.Close();
        }


        [EdenResponse]
        public int CreateLobby(PeerId clientId)
        {
            if (rooms.Count >= 100000)
            {
                // return EdenData.Error("There is no room remain");
                return -1;
            }
            
            int roomNum;
            do
            {
                roomNum = (int)(DateTime.Now.Ticks % 100000L);
            } while (rooms.ContainsKey(roomNum));
            
            
            rooms.Add(roomNum, new Room {host = new NatPeer(), createdTime = DateTime.Now});
            
            return roomNum;
        }
        

        [EdenReceive]
        public void DestroyLobby(PeerId clientId, int roomNumber)
        {

            if (rooms.ContainsKey(roomNumber))
            {
                //게임 호스트만 삭제가능하다
                if (clientId.Ip == rooms[roomNumber].host.RemoteEndPoint.Ip)
                {
                    rooms.Remove(roomNumber);
                }
            }
        }

        [EdenNatRelay]
        private NatPeer? NatRequestEvent(NatPeer peer, string additionalData)
        {
            try
            {
                string type = additionalData.Split("/")[0];
                int roomNumber = int.Parse(additionalData.Split("/")[1]);
                
                Console.WriteLine($"{additionalData}");
                if (type == "host")
                {
                    if (rooms.ContainsKey(roomNumber))
                    {
                        rooms[roomNumber].host = peer;
                        Console.WriteLine($"Host Registered {peer.LocalEndPoint}/{peer.RemoteEndPoint}");
                    }
                }
                else if(type == "client")
                {
                    if (rooms.TryGetValue(roomNumber, out var hostNatPeer))
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

        public static Dictionary<string,object> StringToAddress(string address)
        {
            var splitAddress = address.Split(':');
            var sendData = new Dictionary<string, object>
            {
                ["address"] = splitAddress[0],
                ["port"] = Int32.Parse(splitAddress[1])
            };
            return sendData;
        }
        
    }
}
