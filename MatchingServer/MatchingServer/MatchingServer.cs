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
        public EdenNetServer server;
        public Dictionary<int, string> room;
        public Queue<string> matchQueue;

        public MatchingServer(EdenNetServer server)
        {
            this.server = server;
            room = new Dictionary<int, string>();
            matchQueue = new Queue<string>();
        }

        public void Run()
        {
            server.AddReceiveEvent("MatchMake", MatchMake);
            server.AddResponse("CreateLobby", CreateLobby);
            server.AddResponse("DestroyLobby", DestroyLobby);
            server.AddResponse("GetRoomAddress", GetRoomAddress);

            Task.Run(async () =>
            {
                while (true)
                {
                    if (matchQueue.Count >= 4)
                    {
                        List<string> group = new List<string>();
                        string host = matchQueue.Dequeue();
                        group.Add(host);
                        if (!server.StillConnected(host))
                            continue;
                        server.Send("StartHost", host);

                        for (int i = 0; i < 3; i++)
                        {
                            string client = matchQueue.Dequeue();
                            group.Add(client);
                            if (!server.StillConnected(client))
                            {
                                foreach (string mem in group)
                                    matchQueue.Enqueue(mem);
                                continue;
                            }
                            server.Send("HostAddress", client, host);
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
            if(room.Count >= 100000)
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
                return new EdenData("wrong number");
            int roomNum = data.Get<int>();
            var address = room[roomNum].Split(':');
            var sendData = new Dictionary<string, object>
            {
                ["address"] = address[0],
                ["port"] = Int32.Parse(address[1])
            };
            return new EdenData(sendData);
        }

        public void MatchMake(string client_id, EdenData data)
        {
            matchQueue.Enqueue(client_id);
        }

    }
}
