using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            public string id;
            public bool canceled = false;
        }

        public EdenNetServer server;
        public Dictionary<int, string> room;
        public Queue<MatchInfo> matchQueue;
        public Dictionary<string, MatchInfo> matchMap;

        public MatchingServer(EdenNetServer server)
        {
            this.server = server;
            room = new Dictionary<int, string>();
            matchQueue = new Queue<MatchInfo>();
        }

        public void Run()
        {
            server.AddReceiveEvent("MatchMake", MatchMake);
            server.AddReceiveEvent("CancelMatchMake", CancelMatchMake);
            server.AddResponse("CreateLobby", CreateLobby);
            server.AddResponse("DestroyLobby", DestroyLobby);
            server.AddResponse("GetRoomAddress", GetRoomAddress);

            Task.Run(async () =>
            {
                while (true)
                {
                    if (matchQueue.Count >= 4)
                    {
                        List<MatchInfo> matchGroup = new List<MatchInfo>();
                        for (int i = 0; i < 4; i++)
                        {
                            var clientInfo = matchQueue.Dequeue();
                            if (!clientInfo.canceled && server.StillConnected(clientInfo.id))
                                matchGroup.Add(clientInfo);
                        }
                        if (matchGroup.Count != 4)
                        {
                            foreach (var clientInfo in matchGroup)
                                matchQueue.Enqueue(clientInfo);
                            continue;
                        }

                        var hostAddress = StringToAddress(matchGroup[0].id);
                        server.Send("StartHost", matchGroup[0].id);
                        for (int i = 1; i < 4; i++)
                        {
                            server.Send("StartClient", matchGroup[i].id, hostAddress);
                        }
                        for (int i = 0; i < 4; i++)
                        {
                            if (matchMap.ContainsKey(matchGroup[i].id))
                                matchMap.Remove(matchGroup[i].id);
                            //server.DisconnectClient(matchGroup[i].id);
                        }
                    }
                    await Task.Delay(100);
                }
            });

            server.Listen(99999, (string client_id) =>
            {
            });
        }

        public void Close()
        {
            server.Close();
        }


        public EdenData CreateLobby(string client_id, EdenData data)
        {
            if (room.Count >= 100000)
            {
                return new EdenData(-1);
            }

            //int roomNum;
            //do
            //{
            //    roomNum = (int)(DateTime.Now.Ticks % 100000L);
            //} while (!room.ContainsKey(roomNum));

            room.Add(12345, client_id);

            return new EdenData(12345);
        }

        public EdenData DestroyLobby(string client_id, EdenData data)
        {
            int roomNum = data.Get<int>();

            if (room.ContainsKey(roomNum))
            {
                if (client_id == room[roomNum])
                {
                    room.Remove(roomNum);
                    return new EdenData("Room destroyed");
                }
                else return new EdenData("Not permitted access");
            }
            else return new EdenData("Room deos not exist");

        }

        public EdenData GetRoomAddress(string client_id, EdenData data)
        {
            if (data.type != EdenData.Type.SINGLE)
                return new EdenData(new EdenError("ERR:Unauthorized Access"));
            int roomNum = data.Get<int>();
            if (room.ContainsKey(roomNum))
                return StringToAddress(room[roomNum]);
            else
                return new EdenData(new EdenError("ERR:Wrong Lobby Number"));
        }

        public void MatchMake(string client_id, EdenData data)
        {
            MatchInfo matchInfo = new MatchInfo { id = client_id, canceled = false };
            matchQueue.Enqueue(matchInfo);
            matchMap.Add(matchInfo.id, matchInfo);
        }

        public void CancelMatchMake(string client_id, EdenData data)
        {
            if(matchMap.ContainsKey(client_id))
            {
                matchMap[client_id].canceled = true;
            }
        }

        public static EdenData StringToAddress(string address)
        {
            var splitAddress = address.Split(':');
            var sendData = new Dictionary<string, object>
            {
                ["address"] = splitAddress[0],
                ["port"] = Int32.Parse(splitAddress[1])
            };
            return new EdenData(sendData);
        }

    }
}
