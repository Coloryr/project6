using Fleck;
using Lib;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server
{
    class WebSocket
    {
        private static WebSocketServer Server;
        private static readonly ConcurrentDictionary<int, IWebSocketConnection> Clients = new();
        private static readonly ConcurrentDictionary<string, string> Tokens = new();
        public static bool IsActive { get; set; }//是否在运行

        public static bool CheckHand(IDictionary<string, string> list)
        {
            if (list.ContainsKey(LoginArg.Key) && list[LoginArg.Key] == LoginArg.Value)
            {
                return true;
            }
            return false;
        }

        public static bool CheckLogin(string user, string token)
        {
            if (Tokens.ContainsKey(user) && Tokens[user] == token)
                return true;
            return false;
        }

        public static void Start()
        {
            try
            {
                ServerMain.LogOut("WebSocket服务器正在启动");
                FleckLog.Level = LogLevel.Error;
                Server = new WebSocketServer("ws://" + ServerMain.Config.IP + ":" + ServerMain.Config.Port);
                Server.Start(Socket =>
                {
                    Socket.OnOpen = () =>
                    {
                        IDictionary<string, string> list = Socket.ConnectionInfo.Headers;
                        if (!CheckHand(list))
                        {
                            Socket.Close();
                            return;
                        }
                        Clients.TryAdd(Socket.ConnectionInfo.ClientPort, Socket);
                    };
                    Socket.OnClose = () =>
                    {
                        Clients.TryRemove(Socket.ConnectionInfo.ClientPort, out var item);
                    };
                    Socket.OnMessage = message =>
                    {
                        Do(Socket, message);
                    };
                });
                IsActive = true;
                ServerMain.LogOut("WebSocket服务器已启动");
            }
            catch (Exception e)
            {
                ServerMain.LogError(e);
            }
        }

        public static void SendAll(object obj)
        {
            var data = JsonConvert.SerializeObject(obj);
            foreach (var item in Clients.Values)
            {
                item.Send(data);
            }
        }

        public static void Send(IWebSocketConnection client, object obj)
        {
            var data = JsonConvert.SerializeObject(obj);
            client.Send(data);
        }

        public static void Stop()
        {
            if (IsActive)
            {
                ServerMain.LogOut("WebSocket服务器正在关闭");
                IsActive = false;
                Server.Dispose();
                ServerMain.LogOut("WebSocket服务器已关闭");
            }
        }

        public static void AddGroup(string group)
        {
            var send = new DataPackObj
            {
                Type = DataType.AddGroup,
                Res = true,
                Data = group
            };
            Task.Run(() =>
            {
                foreach (var item in Clients.Values)
                {
                    Send(item, send);
                }
            });
        }

        public static void MoveGroup(string uuid, string group)
        {
            var send = new DataPackObj
            {
                Type = DataType.MoveGroup,
                Res = true,
                Data = group,
                Data1 = uuid
            };
            Task.Run(() =>
            {
                foreach (var item in Clients.Values)
                {
                    Send(item, send);
                }
            });
        }

        public static void UpdateItem(string group, ItemSaveObj obj)
        {
            var send = new DataPackObj
            {
                Type = DataType.Updata,
                Res = true,
                Data = group,
                Data1 = obj
            };
            Task.Run(() =>
            {
                foreach (var item in Clients.Values)
                {
                    Send(item, send);
                }
            });
        }

        public static void Do(IWebSocketConnection client, string data)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<DataPackObj>(data);
                string User = obj.User;
                string Token = obj.Token;
                switch (obj.Type)
                {
                    case DataType.CheckLogin:
                        if (ServerMain.Config.User.ContainsKey(User))
                        {
                            if (Tokens.ContainsKey(User))
                            {
                                Send(client, new DataPackObj
                                {
                                    Type = DataType.CheckLogin,
                                    Res = Tokens[User] == Token
                                });
                                break;
                            }
                        }
                        Send(client, new DataPackObj
                        {
                            Type = DataType.CheckLogin,
                            Res = false
                        });
                        break;
                    case DataType.Login:
                        if (ServerMain.Config.User.ContainsKey(User))
                        {
                            string data1 = (string)obj.Data;
                            string data2 = ServerMain.Config.User[data1];
                            if (data2 != data1)
                            {
                                Send(client, new DataPackObj
                                {
                                    Type = DataType.Login,
                                    Res = false,
                                    Data = "账户或密码错误"
                                });
                            }
                            else
                            {
                                string uuid = new Guid().ToString();
                                Send(client, new DataPackObj
                                {
                                    Type = DataType.Login,
                                    Res = true,
                                    Data = uuid
                                });
                                if (Tokens.ContainsKey(User))
                                {
                                    Tokens[User] = uuid;
                                }
                                else
                                {
                                    Tokens.TryAdd(User, uuid);
                                }
                                Clients.TryAdd(client.ConnectionInfo.ClientPort, client);
                            }
                        }
                        else
                        {
                            Send(client, new DataPackObj
                            {
                                Type = DataType.Login,
                                Res = false,
                                Data = "账户或密码错误"
                            });
                        }
                        break;
                    case DataType.GetGroups:
                        if (!CheckLogin(User, Token))
                        {
                            Send(client, new DataPackObj
                            {
                                Type = DataType.GetGroups,
                                Res = false,
                                Data = "账户错误"
                            });
                        }
                        else
                        {
                            Send(client, new DataPackObj
                            {
                                Type = DataType.GetGroups,
                                Res = true,
                                Data = ServerMain.SaveData.Groups
                            });
                        }
                        break;
                    case DataType.GetGroupInfo:
                        if (!CheckLogin(User, Token))
                        {
                            Send(client, new DataPackObj
                            {
                                Type = DataType.GetGroups,
                                Res = false,
                                Data = "账户错误"
                            });
                        }
                        else
                        {
                            string temp = (string)obj.Data1;
                            if (ServerMain.SaveData.Groups.ContainsKey(temp))
                            {
                                var group = ServerMain.SaveData.Groups[temp];
                                Send(client, new DataPackObj
                                {
                                    Type = DataType.GetGroupInfo,
                                    Res = true,
                                    Data = group
                                });
                            }
                            else
                            {
                                Send(client, new DataPackObj
                                {
                                    Type = DataType.GetGroupInfo,
                                    Res = false,
                                    Data = $"没有组{temp}"
                                });
                            }
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                ServerMain.LogError(e);
            }
        }
    }
}
