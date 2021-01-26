using Fleck;
using Lib;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

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
            if (!list.ContainsKey(LoginArg.Key) || list[LoginArg.Key] == LoginArg.Value)
            {
                return false;
            }
            if (!list.ContainsKey(LoginArg.UserKey) && !list.ContainsKey(LoginArg.TokenKey))
            {
                return false;
            }
            return true;
        }

        public static bool CheckLogin(IDictionary<string, string> list)
        {
            string User = list[LoginArg.UserKey];
            string Token = list[LoginArg.TokenKey];
            if (Tokens.ContainsKey(User) && Tokens[User] == Token)
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
                        }
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

        public static void Do(IWebSocketConnection client, string data)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<DataPackObj>(data);
                IDictionary<string, string> list = client.ConnectionInfo.Headers;
                switch (obj.Type)
                {
                    case DataType.Login:
                        string User = list[LoginArg.UserKey];
                        if (ServerMain.Config.User.ContainsKey(User))
                        {
                            string data1 = (string)obj.Data;
                            string data2 = ServerMain.Config.User[data1];
                            if (data2 != data1)
                            {
                                Send(client, new DataPackObj
                                {
                                    Type = DataType.Login,
                                    Data = false,
                                    Data1 = "账户或密码错误"
                                });
                            }
                            else
                            {
                                string uuid = new Guid().ToString();
                                Send(client, new DataPackObj
                                {
                                    Type = DataType.Login,
                                    Data = true,
                                    Data1 = uuid
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
                                Data = false,
                                Data1 = "账户或密码错误"
                            });
                        }
                        break;
                    case DataType.GetList:
                        if (!CheckLogin(list))
                        {
                            Send(client, new DataPackObj
                            {
                                Type = DataType.GetList,
                                Data = false,
                                Data1 = "账户错误"
                            });
                        }
                        else
                        {
                            Send(client, new DataPackObj
                            {
                                Type = DataType.GetList,
                                Data = true,
                                Data1 = "账户错误"
                            });
                        }
                        break;
                    case DataType.GetInfo:

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
