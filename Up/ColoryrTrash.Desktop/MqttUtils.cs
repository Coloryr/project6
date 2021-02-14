using Lib;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Formatter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ColoryrTrash.Desktop
{
    public class MqttUtils
    {
        private readonly MqttClient Client;
        private string SelfServerTopic;
        private string SelfClientTopic;
        private bool IsConnecting;

        public bool isok;
        public MqttUtils()
        {
            try
            {
                Client = new MqttFactory().CreateMqttClient() as MqttClient;
                Client.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnMqttClientConnected);
                Client.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnMqttClientDisConnected);
                Client.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(OnSubscriberMessageReceived);
                isok = true;
            }
            catch
            {
                isok = false;
            }
        }

        private void OnSubscriberMessageReceived(MqttApplicationMessageReceivedEventArgs arg)
        {
            try
            {
                if (arg.ApplicationMessage.Topic == SelfServerTopic)
                {
                    string Message = Encoding.UTF8.GetString(arg.ApplicationMessage.Payload);
                    var obj = JsonConvert.DeserializeObject<DataPackObj>(Message);
                    if (obj.Type == DataType.Login)
                    {
                        if (obj.Res)
                        {
                            App.Config.Token = obj.Data;
                            App.Save();
                            App.Start();
                            App.IsLogin = true;
                            App.LoginWindows.LoginClose();
                            App.ShowA("登录", "登录成功");
                        }
                        else
                        {
                            App.IsLogin = false;
                            App.ShowB("登录", obj.Data);
                        }
                        return;
                    }
                    else if (obj.Type == DataType.CheckLogin)
                    {
                        if (obj.Res == false)
                        {
                            App.ShowB("自动登录", "自动登录失败");
                        }
                        else
                        {
                            App.Start();
                            App.IsLogin = true;
                            App.LoginWindows.LoginClose();
                            App.ShowA("自动登录", "已自动登录");
                        }
                        return;
                    }
                    if (!obj.Res)
                    {
                        App.IsLogin = false;
                        App.ShowB("登录", obj.Data);
                        App.Login();
                        return;
                    }
                    switch (obj.Type)
                    {
                        case DataType.GetTrashGroups:
                            var list = JsonConvert.DeserializeObject<List<string>>(obj.Data);
                            App.ListWindow_?.SetList(list);
                            break;
                        case DataType.GetTrashGroupInfo:
                            if (obj.Data == null)
                            {
                                App.ShowB("获取垃圾桶组内容错误", obj.Data1);
                            }
                            else
                            {
                                var list1 = JsonConvert.DeserializeObject<TrashDataSaveObj>(obj.Data);
                                App.ListWindow_?.SetInfo(list1);
                            }
                            break;
                        case DataType.AddTrashGroup:
                            App.ShowA("添加垃圾桶组", obj.Data);
                            break;
                        case DataType.RenameTrashGroup:
                            App.ShowA("修改垃圾桶组", obj.Data);
                            break;
                        case DataType.SetTrashNick:
                            App.ShowA("设置垃圾桶备注", obj.Data);
                            break;
                        case DataType.CheckTrashUUID:
                            App.ShowA("垃圾桶UUID检查", obj.Data);
                            break;
                        case DataType.GetUserGroups:
                            var list2 = JsonConvert.DeserializeObject<List<string>>(obj.Data);
                            App.UserListWindow_?.SetList(list2);
                            break;
                        case DataType.GetUserGroupInfo:
                            if (obj.Data == null)
                            {
                                App.ShowB("获取账户组内容错误", obj.Data1);
                            }
                            else
                            {
                                var list3 = JsonConvert.DeserializeObject<UserDataSaveObj>(obj.Data);
                                App.UserListWindow_?.SetInfo(list3);
                            }
                            break;
                        case DataType.AddUser:
                            App.ShowA("添加账户", obj.Data);
                            break;
                        case DataType.RenameUserGroup:
                            App.ShowA("修改账户组名字", obj.Data);
                            break;
                    }
                }
                else if (arg.ApplicationMessage.Topic == DataArg.TopicDesktopServer)
                {
                    string Message = Encoding.UTF8.GetString(arg.ApplicationMessage.Payload);
                    var obj = JsonConvert.DeserializeObject<DataPackObj>(Message);
                    switch (obj.Type)
                    {
                        case DataType.AddTrashGroup:
                            App.ListWindow_.AddGroup(obj.Data);
                            break;
                        case DataType.RenameTrashGroup:
                            App.ListWindow_.RenameGroup(obj.Data, obj.Data1);
                            break;
                        case DataType.MoveTrashGroup:
                            App.ListWindow_.MoveGroup(obj.Data, obj.Data1);
                            break;
                        case DataType.UpdataTrash:
                            var obj1 = JsonConvert.DeserializeObject<TrashSaveObj>(obj.Data1);
                            App.ListWindow_.Updata(obj.Data, obj1);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                App.LogError(ex);
            }
        }

        internal void AddUserGroup(string res)
        {
            throw new NotImplementedException();
        }

        private void OnMqttClientDisConnected(MqttClientDisconnectedEventArgs arg)
        {
            App.Log("服务器连接断开");
            if (!IsConnecting)
                App.DisConnect();
        }

        private void OnMqttClientConnected(MqttClientConnectedEventArgs arg)
        {
            App.Log("服务器已连接");
        }

        private void Send(string message)
        {
            if (Client.IsConnected)
            {
                var obj = new MqttApplicationMessageBuilder()
                    .WithTopic(SelfClientTopic)
                    .WithPayload(Encoding.UTF8.GetBytes(message))
                    .WithExactlyOnceQoS()
                    .WithRetainFlag()
                    .Build();
                Client.PublishAsync(obj);
            }
        }

        public bool Check()
        {
            return Client.IsConnected;
        }

        public async Task<bool> Start()
        {
            try
            {
                IsConnecting = true;
                if (Client.IsConnected)
                    await Stop();
                var options = new MqttClientOptions
                {
                    ProtocolVersion = MqttProtocolVersion.V311,
                    ChannelOptions = new MqttClientTcpOptions
                    {
                        Server = App.Config.IP,
                        Port = App.Config.Port
                    }
                };
                if (options.ChannelOptions == null)
                {
                    throw new InvalidOperationException();
                }
                options.Credentials = new MqttClientCredentials
                {
                    Username = App.Config.User
                };

                options.CleanSession = true;
                options.KeepAlivePeriod = TimeSpan.FromSeconds(5);
                options.ClientId = App.Config.User;

                await Client.ConnectAsync(options);
                SelfServerTopic = DataArg.TopicDesktopServer + "/" + App.Config.User;
                SelfClientTopic = DataArg.TopicDesktopClient + "/" + App.Config.User;
                await Client.SubscribeAsync(SelfServerTopic);
                await Client.SubscribeAsync(DataArg.TopicDesktopServer);
                IsConnecting = false;
                return true;
            }
            catch (Exception e)
            {
                App.LogError(e);
            }
            return false;
        }

        public async Task Stop()
        {
            try
            {
                await Client.DisconnectAsync();
            }
            catch (Exception e)
            {
                App.LogError(e);
            }
        }

        public void CheckLogin(string token)
        {
            var obj = new DataPackObj
            {
                Token = token,
                Type = DataType.CheckLogin
            };
            Send(JsonConvert.SerializeObject(obj));
        }

        public void Login(string pass)
        {
            var obj = new DataPackObj
            {
                Type = DataType.Login,
                Data = Tools.GenSHA1(pass)
            };
            Send(JsonConvert.SerializeObject(obj));
        }

        public void GetTrashGroups()
        {
            var obj = new DataPackObj
            {
                Token = App.Config.Token,
                Type = DataType.GetTrashGroups
            };
            Send(JsonConvert.SerializeObject(obj));
        }
        public void GetTrashGroupInfo(string group)
        {
            var obj = new DataPackObj
            {
                Token = App.Config.Token,
                Type = DataType.GetTrashGroupInfo,
                Data = group
            };
            Send(JsonConvert.SerializeObject(obj));
        }
        public void AddTrashGroup(string group)
        {
            var obj = new DataPackObj
            {
                Token = App.Config.Token,
                Type = DataType.AddTrashGroup,
                Data = group
            };
            Send(JsonConvert.SerializeObject(obj));
        }
        public void RenameTrashGroup(string old, string res)
        {
            var obj = new DataPackObj
            {
                Token = App.Config.Token,
                Type = DataType.RenameTrashGroup,
                Data = old,
                Data1 = res
            };
            Send(JsonConvert.SerializeObject(obj));
        }
        public void MoveTrashGroup(string uuid, string res)
        {
            var obj = new DataPackObj
            {
                Token = App.Config.Token,
                Type = DataType.MoveTrashGroup,
                Data = uuid,
                Data1 = res
            };
            Send(JsonConvert.SerializeObject(obj));
        }
        public void SetTrashNick(string uuid, string res)
        {
            var obj = new DataPackObj
            {
                Token = App.Config.Token,
                Type = DataType.SetTrashNick,
                Data = uuid,
                Data1 = res
            };
            Send(JsonConvert.SerializeObject(obj));
        }
        public void CheckTrashUUID(string uuid)
        {
            var obj = new DataPackObj
            {
                Token = App.Config.Token,
                Type = DataType.CheckTrashUUID,
                Data = uuid
            };
            Send(JsonConvert.SerializeObject(obj));
        }
        public void GetUserGroupInfo(string group)
        {
            var obj = new DataPackObj
            {
                Token = App.Config.Token,
                Type = DataType.GetUserGroupInfo,
                Data = group
            };
            Send(JsonConvert.SerializeObject(obj));
        }

        public void GetUserGroups()
        {
            var obj = new DataPackObj
            {
                Token = App.Config.Token,
                Type = DataType.GetUserGroups
            };
            Send(JsonConvert.SerializeObject(obj));
        }
        public void AddUser(string id, string pass)
        {
            var obj = new DataPackObj
            {
                Token = App.Config.Token,
                Type = DataType.AddUser,
                Data = id,
                Data1 = pass
            };
            Send(JsonConvert.SerializeObject(obj));
        }
        public void RenameUserGroup(string old, string group)
        {
            var obj = new DataPackObj
            {
                Token = App.Config.Token,
                Type = DataType.RenameUserGroup,
                Data = old,
                Data1 = group
            };
            Send(JsonConvert.SerializeObject(obj));
        }
    }
}
