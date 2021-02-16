using Lib;
using MQTTnet;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;

namespace ColoryrTrash.Server.Mqtt
{
    class DesktopServer
    {
        private static readonly ConcurrentDictionary<string, string> Tokens = new();
        public static void UpdateUserItem(string group, UserSaveObj item)
        {
            var send = new DataPackObj
            {
                Type = DataType.SetUser,
                Res = true,
                Data = group,
                Data1 = JsonConvert.SerializeObject(item)
            };
            Task.Run(() => SendAll(send));
        }
        public static void RenameUserGroup(string old, string group)
        {
            var send = new DataPackObj
            {
                Type = DataType.RenameUserGroup,
                Res = true,
                Data = old,
                Data1 = group
            };
            Task.Run(() => SendAll(send));
        }
        public static void AddUserGroup(string group)
        {
            var send = new DataPackObj
            {
                Type = DataType.AddUserGroup,
                Res = true,
                Data = group
            };
            Task.Run(() => SendAll(send));
        }
        public static void MoveUserGroup(string id, string group)
        {
            var send = new DataPackObj
            {
                Type = DataType.MoveUserGroup,
                Res = true,
                Data = id,
                Data1 = group
            };
            Task.Run(() => SendAll(send));
        }

        public static void AddTrashGroup(string group)
        {
            var send = new DataPackObj
            {
                Type = DataType.AddTrashGroup,
                Res = true,
                Data = group
            };
            Task.Run(() => SendAll(send));
        }

        public static void RenameTrashGroup(string old, string group)
        {
            var send = new DataPackObj
            {
                Type = DataType.RenameTrashGroup,
                Res = true,
                Data = old,
                Data1 = group
            };
            Task.Run(() => SendAll(send));
        }

        public static void MoveTrashGroup(string uuid, string group)
        {
            var send = new DataPackObj
            {
                Type = DataType.MoveTrashGroup,
                Res = true,
                Data = uuid,
                Data1 = group
            };
            Task.Run(() => SendAll(send));
        }

        public static void UpdateTrashItem(string group, TrashSaveObj obj)
        {
            var send = new DataPackObj
            {
                Type = DataType.UpdataTrash,
                Res = true,
                Data = group,
                Data1 = JsonConvert.SerializeObject(obj)
            };
            Task.Run(() => SendAll(send));
        }
        public static void AddUser(string group)
        {
            var send = new DataPackObj
            {
                Type = DataType.AddUser,
                Res = true,
                Data = group
            };
            Task.Run(() => SendAll(send));
        }
        public static void Remove(string id)
        {
            var send = new DataPackObj
            {
                Type = DataType.DeleteUser,
                Res = true,
                Data = id
            };
            Task.Run(() => SendAll(send));
        }
        public static async void SendAll(object data)
        {
            var temp = JsonConvert.SerializeObject(data);
            var message = new MqttApplicationMessage()
            {
                Topic = DataArg.TopicDesktopServer,
                Payload = Encoding.UTF8.GetBytes(temp)
            };
            await ThisMqttServer.PublishAsync(message);
        }

        public static async void Send(string uuid, object obj)
        {
            string data = JsonConvert.SerializeObject(obj);
            var message = new MqttApplicationMessage()
            {
                Topic = DataArg.TopicDesktopServer + "/" + uuid,
                Payload = Encoding.UTF8.GetBytes(data)
            };
            await ThisMqttServer.PublishAsync(message);
        }
        public static void DesktopReceived(string ClientId, byte[] Payload)
        {
            string data = Encoding.UTF8.GetString(Payload);
            var obj = JsonConvert.DeserializeObject<DataPackObj>(data);
            string User = ClientId;
            if (User == null)
                return;
            string Token = obj.Token;
            if (obj.Type == DataType.CheckLogin)
            {
                var res = false;
                if (Tokens.ContainsKey(User))
                {
                    res = Tokens[User] == Token;
                }
                Send(ClientId, new DataPackObj
                {
                    Type = DataType.CheckLogin,
                    Res = res
                });
                return;
            }
            else if (obj.Type == DataType.Login)
            {
                if (obj.Data != ServerMain.Config.User[User])
                {
                    Send(ClientId, new DataPackObj
                    {
                        Type = DataType.Login,
                        Res = false,
                        Data = "账户或密码错误"
                    });
                }
                else
                {
                    string uuid = Guid.NewGuid().ToString();
                    if (Tokens.ContainsKey(User))
                        Tokens[User] = uuid;
                    else
                        Tokens.TryAdd(User, uuid);
                    Send(ClientId, new DataPackObj
                    {
                        Type = DataType.Login,
                        Res = true,
                        Data = uuid
                    });
                }
                return;
            }
            if (!Tokens.ContainsKey(User) || Tokens[User] != Token)
            {
                Send(ClientId, new DataPackObj
                {
                    Type = DataType.Login,
                    Res = false,
                    Data = "账户错误"
                });
                return;
            }
            string temp = obj.Data;
            string temp1 = obj.Data1;
            switch (obj.Type)
            {
                case DataType.GetTrashGroups:
                    Send(ClientId, new DataPackObj
                    {
                        Type = DataType.GetTrashGroups,
                        Res = true,
                        Data = JsonConvert.SerializeObject(ServerMain.SaveData.Groups.Keys)
                    });
                    break;
                case DataType.GetTrashGroupInfo:
                    if (ServerMain.SaveData.Groups.ContainsKey(temp))
                    {
                        var group = ServerMain.SaveData.Groups[temp];
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.GetTrashGroupInfo,
                            Res = true,
                            Data = JsonConvert.SerializeObject(group)
                        });
                    }
                    else
                    {
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.GetTrashGroupInfo,
                            Res = true,
                            Data1 = $"没有垃圾桶组[{temp}]"
                        });
                    }
                    break;
                case DataType.AddTrashGroup:
                    var res = ServerMain.SaveData.AddGroup(temp);
                    if (res)
                    {
                        ServerMain.UserLogOut($"用户[{User}]创建垃圾桶组[{temp}]");
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.AddTrashGroup,
                            Res = true,
                            Data = $"垃圾桶组[{temp}]创建成功"
                        });
                    }
                    else
                    {
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.AddTrashGroup,
                            Res = true,
                            Data = $"垃圾桶组[{temp}]创建失败"
                        });
                    }
                    break;
                case DataType.RenameTrashGroup:
                    if (ServerMain.SaveData.RenameGroup(temp, temp1))
                    {
                        ServerMain.UserLogOut($"用户[{User}]修改垃圾桶组[{temp}]为[{temp1}]");
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.RenameTrashGroup,
                            Res = true,
                            Data = $"组[{temp}]已重命名为[{temp1}]"
                        });
                    }
                    else
                    {
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.RenameTrashGroup,
                            Res = true,
                            Data = $"垃圾桶组[{temp}]重命名失败"
                        });
                    }
                    break;
                case DataType.MoveTrashGroup:
                    if (ServerMain.SaveData.MoveGroup(temp, temp1))
                    {
                        ServerMain.UserLogOut($"用户[{User}]移动垃圾桶[{temp}]到组[{temp1}]");
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.MoveTrashGroup,
                            Res = true,
                            Data = $"垃圾桶[{temp}]已移动到[{temp1}]"
                        });
                    }
                    else
                    {
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.MoveTrashGroup,
                            Res = true,
                            Data = $"垃圾桶[{temp}]移动失败"
                        });
                    }
                    break;
                case DataType.SetTrashNick:
                    if (ServerMain.SaveData.SetNick(temp, temp1))
                    {
                        ServerMain.UserLogOut($"用户[{User}]设置垃圾桶[{temp}]的备注为[{temp1}]");
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.SetTrashNick,
                            Res = true,
                            Data = $"垃圾桶[{temp}]已设置备注[{temp1}]"
                        });
                    }
                    else
                    {
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.SetTrashNick,
                            Res = true,
                            Data = $"垃圾桶[{temp}]设置备注失败"
                        });
                    }
                    break;
                case DataType.CheckTrashUUID:
                    if (ServerMain.SaveData.CheckUUID(temp))
                    {
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.CheckTrashUUID,
                            Res = true,
                            Data = "UUID已存在"
                        });
                    }
                    else
                    {
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.CheckTrashUUID,
                            Res = true,
                            Data = "UUID不存在"
                        });
                    }
                    break;
                case DataType.AddUser:
                    if (ServerMain.UserData.CheckID(temp))
                    {
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.AddUser,
                            Res = true,
                            Data = "账户已存在"
                        });
                    }
                    else
                    {
                        ServerMain.UserData.AddUser(temp, temp1);
                        ServerMain.UserLogOut($"用户[{User}]添加账户[{temp}]");
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.AddUser,
                            Res = true,
                            Data = "账户创建成功"
                        });
                    }
                    break;
                case DataType.DeleteUser:
                    if (!ServerMain.UserData.CheckID(temp))
                    {
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.DeleteUser,
                            Res = true,
                            Data = "账户不存在"
                        });
                    }
                    else
                    {
                        ServerMain.UserData.RemoveUser(temp);
                        ServerMain.UserLogOut($"用户[{User}]删除账户[{temp}]");
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.DeleteUser,
                            Res = true,
                            Data = "账户删除成功"
                        });
                    }
                    break;
                case DataType.SetUser:
                    if (!ServerMain.UserData.CheckID(temp))
                    {
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.SetUser,
                            Res = true,
                            Data = "账户不存在"
                        });
                    }
                    else
                    {
                        ServerMain.UserData.ChangePass(temp, temp1);
                        ServerMain.UserLogOut($"用户[{User}]修改账户[{temp}]密码");
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.SetUser,
                            Res = true,
                            Data = $"账户[{temp}]密码已修改"
                        });
                    }
                    break;
                case DataType.AddUserGroup:
                    if (!ServerMain.UserData.AddGroup(temp))
                    {
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.AddUserGroup,
                            Res = true,
                            Data = $"账户组[{temp}]创建失败"
                        });
                    }
                    else
                    {
                        ServerMain.UserLogOut($"用户[{User}]创建账户组[{temp}]");
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.AddUserGroup,
                            Res = true,
                            Data = $"账户组[{temp}]已创建"
                        });
                    }
                    break;
                case DataType.MoveUserGroup:
                    if(!ServerMain.UserData.MoveGroup(temp, temp1))
                    {
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.MoveUserGroup,
                            Res = true,
                            Data = $"账户[{temp}]移动到组[{temp1}]失败"
                        });
                    }
                    else
                    {
                        ServerMain.UserLogOut($"用户[{User}]移动账户[{temp}]到组[{temp1}]");
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.MoveUserGroup,
                            Res = true,
                            Data = $"账户[{temp}]已移动到组[{temp1}]"
                        });
                    }
                    break;
                case DataType.RenameUserGroup:
                    if (!ServerMain.UserData.RenameGroup(temp, temp1))
                    {
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.RenameUserGroup,
                            Res = true,
                            Data = $"账户组[{temp}]重命名失败"
                        });
                    }
                    else
                    {
                        ServerMain.UserLogOut($"用户[{User}]重命名账户组[{temp}]为[{temp1}]");
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.RenameUserGroup,
                            Res = true,
                            Data = $"账户组[{temp}]已重命名为[{temp1}]"
                        });
                    }
                    break;
                case DataType.GetUserGroups:
                    Send(ClientId, new DataPackObj
                    {
                        Type = DataType.GetUserGroups,
                        Res = true,
                        Data = JsonConvert.SerializeObject(ServerMain.UserData.Groups.Keys)
                    });
                    break;
                case DataType.GetUserGroupInfo:
                    if (!ServerMain.UserData.Groups.ContainsKey(temp))
                    {
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.GetUserGroupInfo,
                            Res = true,
                            Data1 = $"账户组[{temp}]不存在"
                        });
                    }
                    else
                    {
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.GetUserGroupInfo,
                            Res = true,
                            Data = JsonConvert.SerializeObject(ServerMain.UserData.Groups[temp])
                        });
                    }
                    break;
                case DataType.GetUserGroupBind:
                    if (!ServerMain.UserData.Groups.ContainsKey(temp))
                    {
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.GetUserGroupBind,
                            Res = true,
                            Data1 = $"账户组[{temp}]不存在"
                        });
                    }
                    else
                    {
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.GetUserGroupBind,
                            Res = true,
                            Data = JsonConvert.SerializeObject(ServerMain.UserData.Groups[temp].Bind)
                        });
                    }
                    break;
                case DataType.SetUserGroupBind:
                    if (!ServerMain.UserData.SetBind(temp, temp1))
                    {
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.SetUserGroupBind,
                            Res = true,
                            Data1 = $"账户组[{temp}]绑定失败"
                        });
                    }
                    else
                    {
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.SetUserGroupBind,
                            Res = true,
                            Data = $"账户组[{temp}]绑定成功"
                        });
                        AppServer.SendAll(new DataPackObj
                        {
                            Type = DataType.SetUserGroupBind,
                            Res = true,
                            Data = temp
                        });
                    }
                    break;
            }
        }
    }
}
