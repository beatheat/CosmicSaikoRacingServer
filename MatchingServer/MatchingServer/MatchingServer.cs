﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public Dictionary<int, string> room;
        public Queue<MatchInfo> matchQueue;
        public Dictionary<string, MatchInfo> matchMap;

        public MatchingServer(EdenUdpServer server)
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
            if (!data.TryGet<string>(out var privateAddress))
                return EdenData.Error("Wrong formatted address number");
              
            int roomNum;
            do
            {
                roomNum = (int)(DateTime.Now.Ticks % 100000L);
            } while (room.ContainsKey(roomNum));

            room.Add(roomNum, privateAddress + "," + clientId);
            var addr = StringToAddress(clientId);
            EdenUdpClient client = new EdenUdpClient(addr["address"].ToString(), int.Parse(addr["port"].ToString()));
            var conn = client.Connect();
            Console.WriteLine(conn);
            return new EdenData(roomNum);
            
            // async void HeartBeat()
            // {
            //     Console.WriteLine("Ready to Login to GameServer");
            //     var address = clientId.Split(":");
            //     EdenUdpClient client = new EdenUdpClient(address[0], int.Parse(address[1]));
            //     bool connection = true;
            //     client.SetDisconnectEvent(() =>
            //     {
            //         connection = false;
            //     });
            //     int count = 0;
            //     while (await client.ConnectAsync() != ConnectionState.OK)
            //     {
            //         Console.WriteLine($"Trying Login {count}");
            //         await Task.Delay(300);
            //         count++;
            //         if (count > 10)
            //         {
            //             Console.WriteLine($"GameServer connection fail on {clientId}");
            //             return;
            //         }
            //     }
            //     Console.WriteLine($"GameServer connection success on {clientId}");
            //     while (connection)
            //     {
            //         await client.SendAsync("HeartBeat");
            //         await Task.Delay(3000);
            //     }
            // }
        }

        public EdenData DestroyLobby(string clientId, EdenData data)
        {
            if (!data.TryGet<int>(out var roomNum))
                return EdenData.Error("Wrong formatted room number");
            if (room.ContainsKey(roomNum))
            {
                if (clientId == room[roomNum])
                {
                    room.Remove(roomNum);
                    return new EdenData("Room destroyed");
                }
                else return new EdenData("Not permitted access");
            }
            else return new EdenData("Room does not exist");

        }

        public EdenData GetRoomAddress(string clientId, EdenData data)
        {
            if (data.type != EdenData.Type.SINGLE)
                return new EdenData(new EdenError("ERR:Unauthorized Access"));
            if(!data.TryGet<int>(out var roomNum))
                return new EdenData(new EdenError("ERR:Unauthorized Access"));
            if (room.ContainsKey(roomNum))
            {
                string privateAddress = room[roomNum].Split(",")[0];
                string publicAddress = room[roomNum].Split(",")[1];
                return new EdenData(new Dictionary<string, object>
                {
                    ["privateAddress"] = privateAddress,
                    ["publicAddress"] = publicAddress
                });
            }
            else
                return new EdenData(new EdenError("ERR:Wrong Lobby Number"));
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
