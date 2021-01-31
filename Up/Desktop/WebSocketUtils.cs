using Lib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using WebSocket4Net;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.ClientEngine;

namespace Desktop
{
    public class WebSocketUtils
    {
        private WebSocket client;

        public bool isok;
        public WebSocketUtils()
        {
            try
            {
                client = new WebSocket(App.Config.Url, 
                    customHeaderItems: new()
                    { 
                        new(LoginArg.Key, LoginArg.Value)
                    });
                client.Opened += websocket_Opened;
                client.Error += websocket_Error;
                client.Closed += websocket_Closed;
                client.MessageReceived += websocket_MessageReceived;
                isok = true;
            }
            catch
            {
                isok = false;
            }
        }

        private void websocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<DataPackObj>(e.Message);
                switch (obj.Type)
                {
                    case DataType.CheckLogin:
                        if (obj.Res == false)
                        {
                            App.Log("自动登录失败");
                        }
                        break;
                    case DataType.Login:
                        if (obj.Res)
                        {
                            App.Config.Token = (string)obj.Data;
                            App.Save();
                        }
                        App.LoginWindows?.LoginRes(obj.Res);
                        break;
                }
            }
            catch (Exception ex)
            {
                App.LogError(ex);
            }
        }

        private void websocket_Error(object sender, ErrorEventArgs e)
        {
            
        }

        private void websocket_Closed(object sender, EventArgs e)
        {
           
        }

        private void websocket_Opened(object sender, EventArgs e)
        {
            
        }

        private bool Check()
        {
            if (client.State == WebSocketState.Open)
            {
                return true;
            }
            else
            {
                Start();
            }
        }

        public bool Start()
        {
            try
            {
                client.Open();
                return true;
            }
            catch (Exception e)
            {
                App.LogError(e);
            }
            return false;
        }

        public void Stop()
        {
            try
            {
                client.Close();
            }
            catch (Exception e)
            {
                App.LogError(e);
            }
            client.Dispose();
        }

        public void CheckLogin(string user, string token)
        {
            var obj = new DataPackObj
            {
                User = user,
                Token = token,
                Type = DataType.CheckLogin
            };
            client.Send(JsonConvert.SerializeObject(obj));
        }

        public void Login(string user, string pass)
        {
            var obj = new DataPackObj
            {
                User = user,
                Type = DataType.Login,
                Data = pass
            };
            client.Send(JsonConvert.SerializeObject(obj));
        }

        public Dictionary<string, DataSaveObj> GetGroups()
        {
            return null;
        }
    }
}
