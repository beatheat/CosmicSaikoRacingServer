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
using EdenNetwork.Udp;

namespace MatchingServer
{
    internal class MatchingServer
    {
        public class MatchInfo
        {
            public string id;
            public bool canceled = false;
        }

        public EdenUdpServer server;
        public Dictionary<int, NatPeer> room;
        public Queue<MatchInfo> matchQueue;
        public Dictionary<string, MatchInfo> matchMap;

        public MatchingServer(EdenUdpServer server)
        {
            this.server = server;
            room = new Dictionary<int, NatPeer>();
            matchQueue = new Queue<MatchInfo>();

        }


        public void Run()
        {
            server.AddReceiveEvent("MatchMake", MatchMake);
            server.AddReceiveEvent("CancelMatchMake", CancelMatchMake);
            server.AddResponse("CreateLobby", CreateLobby);
            server.AddResponse("DestroyLobby", DestroyLobby);

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

            
            server.SetNatRequestEvent(NatRequestEvent);
            server.Listen(99999, (string client_id) =>
            {
            });
        }


        public void Close()
        {
            server.Close();
        }


        public EdenData CreateLobby(string clientId, EdenData data)
        {
            if (room.Count >= 100000)
            {
                return EdenData.Error("There is no room remain");
            }
            
            int roomNum;
            do
            {
                roomNum = (int)(DateTime.Now.Ticks % 100000L);
            } while (room.ContainsKey(roomNum));
            
            
            room.Add(roomNum, new NatPeer{localEndPoint = null!, remoteEndPoint = null!});
            
            return new EdenData(roomNum);
        }
        

        public EdenData DestroyLobby(string clientId, EdenData data)
        {
            if (!data.TryGet<int>(out var roomNum))
                return EdenData.Error("Wrong formatted room number");
            if (room.ContainsKey(roomNum))
            {
                if (clientId == room[roomNum].remoteEndPoint.ToString() ||
                    clientId == room[roomNum].localEndPoint.ToString())
                {
                    room.Remove(roomNum);
                    return new EdenData("Room destroyed");
                }
                else return new EdenData("Not permitted access");
            }
            else return new EdenData("Room does not exist");

        }

        private NatPeer? NatRequestEvent(NatPeer peer, string data)
        {
            try
            {
                string type = data.Split("/")[0];
                int roomNumber = int.Parse(data.Split("/")[1]);
                
                if (type == "host")
                {
                    if (room.ContainsKey(roomNumber))
                        room[roomNumber] = new NatPeer {localEndPoint = peer.localEndPoint, remoteEndPoint = peer.remoteEndPoint};
                }
                else if(type == "client")
                {
                    if (room.TryGetValue(roomNumber, out var hostNatPeer))
                        return hostNatPeer;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return null;
        }

        public void MatchMake(string clientId, EdenData data)
        {
            MatchInfo matchInfo = new MatchInfo { id = clientId, canceled = false };
            matchQueue.Enqueue(matchInfo);
            matchMap.Add(matchInfo.id, matchInfo);
        }

        public void CancelMatchMake(string clientId, EdenData data)
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
