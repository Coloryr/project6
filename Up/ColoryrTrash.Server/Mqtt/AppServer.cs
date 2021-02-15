﻿using Lib;
using MQTTnet;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColoryrTrash.Server.Mqtt
{
    class AppServer
    {
        private static readonly ConcurrentDictionary<string, string> Tokens = new();
        public static async void SendAll(object data)
        {
            var temp = JsonConvert.SerializeObject(data);
            var message = new MqttApplicationMessage()
            {
                Topic = DataArg.TopicAppServer,
                Payload = Encoding.UTF8.GetBytes(temp)
            };
            await ThisMqttServer.PublishAsync(message);
        }

        public static async void Send(string uuid, object obj)
        {
            string data = JsonConvert.SerializeObject(obj);
            var message = new MqttApplicationMessage()
            {
                Topic = DataArg.TopicAppServer + "/" + uuid,
                Payload = Encoding.UTF8.GetBytes(data)
            };
            await ThisMqttServer.PublishAsync(message);
        }
        public static void AppReceived(string ClientId, byte[] Payload)
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
                var pass = ServerMain.UserData.GetPass(User);
                if (pass == null || obj.Data != pass)
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
        }
    }
}