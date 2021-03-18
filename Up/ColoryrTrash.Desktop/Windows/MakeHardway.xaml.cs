using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ColoryrTrash.Desktop.Windows
{
    /// <summary>
    /// MakeHardway.xaml 的交互逻辑
    /// </summary>
    public partial class MakeHardway : Window
    {
        public int[] BaudRates { get; set; }
        private readonly SerialPort Serial;
        private CancellationTokenSource Cancel;
        private bool Open;
        private bool IsConnect;
        private bool IsRead;
        private bool IsSend;
        private bool IsLoad = true;
        public MakeHardway()
        {
            InitializeComponent();
            DataObject.AddPastingHandler(IP0, OnPaste1);
            DataObject.AddPastingHandler(IP1, OnPaste1);
            DataObject.AddPastingHandler(IP2, OnPaste1);
            DataObject.AddPastingHandler(IP3, OnPaste1);
            DataObject.AddPastingHandler(UUID0, OnPaste2);
            DataObject.AddPastingHandler(UUID1, OnPaste2);
            DataObject.AddPastingHandler(UUID2, OnPaste2);
            DataObject.AddPastingHandler(UUID3, OnPaste2);
            DataObject.AddPastingHandler(UUID4, OnPaste2);
            DataObject.AddPastingHandler(UUID5, OnPaste2);
            DataObject.AddPastingHandler(UUID6, OnPaste2);
            DataObject.AddPastingHandler(UUID7, OnPaste2);
            DataObject.AddPastingHandler(UUID8, OnPaste2);
            DataObject.AddPastingHandler(UUID9, OnPaste2);
            DataObject.AddPastingHandler(UUID10, OnPaste2);
            DataObject.AddPastingHandler(UUID11, OnPaste2);
            DataObject.AddPastingHandler(UUID12, OnPaste2);
            DataObject.AddPastingHandler(UUID13, OnPaste2);
            DataObject.AddPastingHandler(UUID14, OnPaste2);
            DataObject.AddPastingHandler(UUID15, OnPaste2);
            Serial = new();
            BaudRates = new int[] { 4800, 9600, 19200, 38400, 115200 };
            ComList.ItemsSource = SerialPort.GetPortNames();
            if (SerialPort.GetPortNames().Length > 0)
            {
                ComList.SelectedItem = SerialPort.GetPortNames()[0];
            }
            DataContext = this;
            BaudRate.SelectedItem = 115200;
            IsLoad = false;
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Serial.Dispose();
            App.MakeHardway_ = null;
        }
        private bool CheckInput(string data)
        {
            return Regex.IsMatch(data, "^[A-Za-z0-9]+$");
        }
        private bool CheckInputNumber(string data)
        {
            return Regex.IsMatch(data, "^[0-9]+$");
        }
        private string GetRandomString()
        {
            byte[] b = new byte[4];
            new RNGCryptoServiceProvider().GetBytes(b);
            Random r = new Random(BitConverter.ToInt32(b, 0));
            string s = null;
            string str = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            for (int i = 0; i < 16; i++)
            {
                s += str.Substring(r.Next(0, str.Length - 1), 1);
            }
            return s;
        }
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (!Open)
            {
                if (BaudRate.SelectedItem == null || ComList.SelectedItem == null)
                {
                    App.ShowA("打开串口", "串口设置错误");
                    return;
                }
                if (Serial.IsOpen)
                {
                    Serial.Close();
                }
                Serial.PortName = ComList.SelectedItem as string;
                Serial.BaudRate = (int)BaudRate.SelectedItem;
                try
                {
                    Serial.Open();
                    Open_Button.Content = "关闭";
                    State.Content = "打开";
                    ComList.IsEnabled = false;
                    BaudRate.IsEnabled = false;
                    State_Led.Fill = Brushes.Blue;
                    Cancel = new();
                    Task.Run(() => TestConnect(), Cancel.Token);
                    Open = true;
                }
                catch (InvalidOperationException)
                {
                    App.ShowB("打开串口", "串口打开失败，被占用");
                }
                catch (Exception ex)
                {
                    App.ShowB("打开串口", "串口打开失败，其他原因");
                    App.LogError(ex);
                }
            }
            else
            {
                Cancel.Cancel();
                Serial.Close();
                Open_Button.Content = "打开";
                State.Content = "断开";
                ComList.IsEnabled = true;
                BaudRate.IsEnabled = true;
                State_Led.Fill = Brushes.Gray;
                Open = false;
                IsConnect = false;
                Group1.IsEnabled =
                Group2.IsEnabled =
                Group3.IsEnabled =
                Group4.IsEnabled =
                Group5.IsEnabled = false;
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            ComList.ItemsSource = SerialPort.GetPortNames();
        }

        private void TestConnect()
        {
            Serial.Write(HardPack.TestPack, 0, 9);
            Thread.Sleep(1000);
            if (Serial.BytesToRead > 0)
            {
                var data = new byte[Serial.BytesToRead];
                Serial.Read(data, 0, Serial.BytesToRead);
                if (HardPack.CheckPack(data))
                {
                    IsConnect = true;
                    Dispatcher.Invoke(() =>
                    {
                        State.Content = "已连接";
                        Group1.IsEnabled =
                        Group2.IsEnabled =
                        Group3.IsEnabled =
                        Group4.IsEnabled =
                        Group5.IsEnabled = true;
                        State_Led.Fill = Brushes.LawnGreen;
                    });
                    ReadUUID_();
                    ReadIP_();
                    //ReadSensor_();
                    ReadSetting_();
                    ReadMqtt_();
                }
                else
                {
                    IsConnect = false;
                    Dispatcher.Invoke(() =>
                    {
                        State.Content = "未连接";
                        State_Led.Fill = Brushes.Red;
                    });
                }
            }
            else
            {
                IsConnect = false;
                Dispatcher.Invoke(() =>
                {
                    State.Content = "未连接";
                    State_Led.Fill = Brushes.Red;
                });
            }
        }
        private void ReadUUID(byte[] data)
        {
            if (data.Length != 16)
            {
                App.ShowB("读信息", "数据包错误");
                return;
            }
            string temp = Encoding.UTF8.GetString(data);
            ToUUIDs(temp);
        }

        private void ToUUIDs(string data)
        {
            int index = 0;
            string temp;
            for (int a = 0; a < data.Length; a++)
            {
                temp = data[a].ToString();
                if (!CheckInput(temp))
                    continue;
                switch (index)
                {
                    case 0:
                        UUID0.Text = temp;
                        break;
                    case 1:
                        UUID1.Text = temp;
                        break;
                    case 2:
                        UUID2.Text = temp;
                        break;
                    case 3:
                        UUID3.Text = temp;
                        break;
                    case 4:
                        UUID4.Text = temp;
                        break;
                    case 5:
                        UUID5.Text = temp;
                        break;
                    case 6:
                        UUID6.Text = temp;
                        break;
                    case 7:
                        UUID7.Text = temp;
                        break;
                    case 8:
                        UUID8.Text = temp;
                        break;
                    case 9:
                        UUID9.Text = temp;
                        break;
                    case 10:
                        UUID10.Text = temp;
                        break;
                    case 11:
                        UUID11.Text = temp;
                        break;
                    case 12:
                        UUID12.Text = temp;
                        break;
                    case 13:
                        UUID13.Text = temp;
                        break;
                    case 14:
                        UUID14.Text = temp;
                        break;
                    case 15:
                        UUID15.Text = temp;
                        return;
                }
                index++;
            }
        }

        private void ToIPs(string data)
        {
            int index = 0;
            string[] temp = data.Split('.');
            foreach (var item in temp)
            {
                if (!CheckInput(item) || item.Length > 3 || int.Parse(item) > 255)
                    continue;
                switch (index)
                {
                    case 0:
                        IP0.Text = item;
                        break;
                    case 1:
                        IP1.Text = item;
                        break;
                    case 2:
                        IP2.Text = item;
                        break;
                    case 3:
                        IP3.Text = item;
                        return;
                }
                index++;
            }
        }
        private string UUIDSplicing()
        {
            string temp = "";
            if (UUID0.Text != "")
                temp += UUID0.Text;
            else
                return null;
            if (UUID1.Text != "")
                temp += UUID1.Text;
            else
                return null;
            if (UUID2.Text != "")
                temp += UUID2.Text;
            else
                return null;
            if (UUID3.Text != "")
                temp += UUID3.Text;
            else
                return null;
            if (UUID4.Text != "")
                temp += UUID4.Text;
            else
                return null;
            if (UUID5.Text != "")
                temp += UUID5.Text;
            else
                return null;
            if (UUID6.Text != "")
                temp += UUID6.Text;
            else
                return null;
            if (UUID7.Text != "")
                temp += UUID7.Text;
            else
                return null;
            if (UUID8.Text != "")
                temp += UUID8.Text;
            else
                return null;
            if (UUID9.Text != "")
                temp += UUID9.Text;
            else
                return null;
            if (UUID10.Text != "")
                temp += UUID10.Text;
            else
                return null;
            if (UUID11.Text != "")
                temp += UUID11.Text;
            else
                return null;
            if (UUID12.Text != "")
                temp += UUID12.Text;
            else
                return null;
            if (UUID13.Text != "")
                temp += UUID13.Text;
            else
                return null;
            if (UUID14.Text != "")
                temp += UUID14.Text;
            else
                return null;
            if (UUID15.Text != "")
                temp += UUID15.Text;
            else
                return null;
            return temp;
        }

        private void UUID0_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoad)
                return;
            TextBox box = sender as TextBox;
            if (box == null)
                return;
            if (box.Text.Length > 1)
            {
                box.Text = box.Text[0].ToString();
            }
            else if (box.Text.Length == 1)
            {
                if (!CheckInput(box.Text))
                {
                    box.Text = "";
                }
                else
                {
                    if (box.Name == "UUID0")
                    {
                        UUID1.Focus();
                    }
                    else if (box.Name == "UUID1")
                    {
                        UUID2.Focus();
                    }
                    else if (box.Name == "UUID2")
                    {
                        UUID3.Focus();
                    }
                    else if (box.Name == "UUID3")
                    {
                        UUID4.Focus();
                    }
                    else if (box.Name == "UUID4")
                    {
                        UUID5.Focus();
                    }
                    else if (box.Name == "UUID5")
                    {
                        UUID6.Focus();
                    }
                    else if (box.Name == "UUID6")
                    {
                        UUID7.Focus();
                    }
                    else if (box.Name == "UUID7")
                    {
                        UUID8.Focus();
                    }
                    else if (box.Name == "UUID8")
                    {
                        UUID9.Focus();
                    }
                    else if (box.Name == "UUID9")
                    {
                        UUID10.Focus();
                    }
                    else if (box.Name == "UUID10")
                    {
                        UUID11.Focus();
                    }
                    else if (box.Name == "UUID11")
                    {
                        UUID12.Focus();
                    }
                    else if (box.Name == "UUID12")
                    {
                        UUID13.Focus();
                    }
                    else if (box.Name == "UUID13")
                    {
                        UUID14.Focus();
                    }
                    else if (box.Name == "UUID14")
                    {
                        UUID15.Focus();
                    }
                }
            }
        }

        private void ReadUUID_()
        {
            IsRead = true;
            Dispatcher.Invoke(() => ReadUUID_Button.IsEnabled = false);
            var data = HardPack.MakeReadPack(PackType.UUID);
            Serial.Write(data, 0, 6);
            Thread.Sleep(1000);
            if (Serial.BytesToRead > 0)
            {
                var temp = new byte[Serial.BytesToRead];
                int len = Serial.BytesToRead - 6;
                Serial.Read(temp, 0, Serial.BytesToRead);
                var res = HardPack.CheckType(temp);
                if (res == PackType.UUID)
                {
                    var temp1 = new byte[len];
                    Array.Copy(temp, 6, temp1, 0, len);
                    Dispatcher.Invoke(() => ReadUUID(temp1));
                }
                App.ShowA("读信息", "UUID已读取");
            }
            else
            {
                App.ShowB("读信息", "设备未响应");
            }
            Dispatcher.Invoke(() => ReadUUID_Button.IsEnabled = true);
            IsRead = false;
        }

        private void ReadUUID_Click(object sender, RoutedEventArgs e)
        {
            if (IsRead)
                return;
            if (IsConnect)
            {
                IsRead = true;
                ReadUUID_Button.IsEnabled = false;
                var data = HardPack.MakeReadPack(PackType.UUID);
                Serial.Write(data, 0, 6);
                Task.Run(() =>
                {
                    Thread.Sleep(1000);
                    if (Serial.BytesToRead > 0)
                    {
                        var temp = new byte[Serial.BytesToRead];
                        int len = Serial.BytesToRead - 6;
                        Serial.Read(temp, 0, Serial.BytesToRead);
                        var res = HardPack.CheckType(temp);
                        if (res == PackType.UUID)
                        {
                            var temp1 = new byte[len];
                            Array.Copy(temp, 6, temp1, 0, len);
                            Dispatcher.Invoke(() => ReadUUID(temp1));
                        }
                        App.ShowA("读信息", "UUID已读取");
                    }
                    else
                    {
                        App.ShowB("读信息", "设备未响应");
                    }
                    Dispatcher.Invoke(() => ReadUUID_Button.IsEnabled = true);
                    IsRead = false;
                });
            }
            else
            {
                App.ShowB("读信息", "设备未连接");
            }
        }

        private void Random_Click(object sender, RoutedEventArgs e)
        {
            var temp = GetRandomString();
            ToUUIDs(temp);
        }

        private void CheckUUID_Click(object sender, RoutedEventArgs e)
        {
            string temp = UUIDSplicing();
            if (temp == null)
            {
                App.ShowB("设置", "UUID填写错误");
                return;
            }
            App.MqttUtils.CheckTrashUUID(temp);
        }
        private void SetUUID_Click(object sender, RoutedEventArgs e)
        {
            if (IsSend)
                return;
            if (!IsConnect)
            {
                App.ShowB("设置", "设备未连接");
                return;
            }
            string temp = UUIDSplicing();
            if (temp == null)
            {
                App.ShowB("设置", "UUID填写错误");
                return;
            }
            IsSend = true;
            SetUUID_Button.IsEnabled = false;
            Task.Run(() =>
            {
                var data = HardPack.MakeSetPack(PackType.UUID, Encoding.UTF8.GetBytes(temp));
                Serial.Write(data, 0, data.Length);
                Thread.Sleep(1000);
                if (Serial.BytesToRead <= 0)
                {
                    App.ShowB("设置", "设备未响应");
                    Dispatcher.Invoke(() => SetUUID_Button.IsEnabled = true);
                    IsSend = false;
                    return;
                }
                data = new byte[Serial.BytesToRead];
                Serial.Read(data, 0, Serial.BytesToRead);
                if (HardPack.CheckOK(data))
                {
                    App.ShowA("设置", "UUID已设置");
                }
                else
                {
                    App.ShowB("设置", "UUID设置失败");
                }
                Dispatcher.Invoke(() => SetUUID_Button.IsEnabled = true);
                IsSend = false;
            });
        }

        private void IP0_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoad)
                return;
            TextBox box = sender as TextBox;
            if (box == null)
                return;
            if (box.Text.Length > 3)
            {
                box.Text = "";
                return;
            }
            else
            {
                if (!CheckInputNumber(box.Text))
                {
                    box.Text = "";
                }
                if (box.Text.Length == 0)
                    return;
                int temp = int.Parse(box.Text);
                if (temp > 0xff)
                {
                    box.Text = "";
                }
            }
            if (box.Text.Length == 3)
            {
                if (box.Name == "IP0")
                {
                    IP1.Focus();
                }
                else if (box.Name == "IP1")
                {
                    IP2.Focus();
                }
                else if (box.Name == "IP2")
                {
                    IP3.Focus();
                }
            }
        }

        private void Port0_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoad)
                return;
            TextBox box = sender as TextBox;
            if (box == null)
                return;
            if (box.Text.Length > 5)
            {
                box.Text = "";
            }
            else
            {
                if (!CheckInputNumber(box.Text))
                {
                    box.Text = "";
                    return;
                }
                int temp = int.Parse(box.Text);
                if (temp > 0xffff)
                {
                    box.Text = "";
                }
            }
        }

        private void SetIP_Click(object sender, RoutedEventArgs e)
        {
            if (IsSend)
                return;
            if (!IsConnect)
            {
                App.ShowB("设置", "设备未连接");
                return;
            }
            if (!IPCheck())
            {
                App.ShowB("设置", "地址填写错误");
                return;
            }
            IsSend = true;
            SetIP_Button.IsEnabled = false;
            var temp = new byte[6];
            temp[0] = byte.Parse(IP0.Text);
            temp[1] = byte.Parse(IP1.Text);
            temp[2] = byte.Parse(IP2.Text);
            temp[3] = byte.Parse(IP3.Text);
            var temp2 = HardPack.IntToByte(int.Parse(Port0.Text));
            temp[4] = temp2[0];
            temp[5] = temp2[1];
            Task.Run(() =>
            {
                var data = HardPack.MakeSetPack(PackType.IP, temp);
                Serial.Write(data, 0, data.Length);
                Thread.Sleep(1000);
                if (Serial.BytesToRead <= 0)
                {
                    App.ShowB("设置", "设备未响应");
                    Dispatcher.Invoke(() => SetIP_Button.IsEnabled = true);
                    IsSend = false;
                    return;
                }
                data = new byte[Serial.BytesToRead];
                Serial.Read(data, 0, Serial.BytesToRead);
                if (HardPack.CheckOK(data))
                {
                    App.ShowA("设置", "地址已设置");
                }
                else
                {
                    App.ShowB("设置", "地址设置失败");
                }
                Dispatcher.Invoke(() => SetIP_Button.IsEnabled = true);
                IsSend = false;
            });
        }

        private bool IPCheck()
        {
            if (IP0.Text.Length == 0 || IP1.Text.Length == 0 || IP2.Text.Length == 0 || IP3.Text.Length == 0 || Port0.Text.Length == 0)
                return false;
            if (!CheckInputNumber(IP0.Text) || !CheckInputNumber(IP1.Text) || !CheckInputNumber(IP2.Text) || !CheckInputNumber(IP3.Text) || !CheckInputNumber(Port0.Text))
                return false;
            if (int.Parse(IP0.Text) > 0xff || int.Parse(IP1.Text) > 0xff || int.Parse(IP2.Text) > 0xff || int.Parse(IP3.Text) > 0xff || int.Parse(Port0.Text) > 0xffff)
                return false;
            return true;
        }
        private void ReadIP_()
        {
            IsRead = true;
            Dispatcher.Invoke(() => ReadIP_Button.IsEnabled = false);
            var data = HardPack.MakeReadPack(PackType.IP);
            Serial.Write(data, 0, 6);
            Thread.Sleep(1000);
            if (Serial.BytesToRead > 0)
            {
                var temp = new byte[Serial.BytesToRead];
                int len = Serial.BytesToRead - 6;
                Serial.Read(temp, 0, Serial.BytesToRead);
                var res = HardPack.CheckType(temp);
                if (res == PackType.IP)
                {
                    var temp1 = new byte[len];
                    Array.Copy(temp, 6, temp1, 0, len);
                    Dispatcher.Invoke(() => ReadIP(temp1));
                }
                App.ShowA("读信息", "地址已读取");
            }
            else
            {
                App.ShowB("读信息", "设备未响应");
            }
            Dispatcher.Invoke(() => ReadIP_Button.IsEnabled = true);
            IsRead = false;
        }

        private async void ReadIP_Click(object sender, RoutedEventArgs e)
        {
            if (IsRead)
                return;
            if (IsConnect)
            {
                IsRead = true;
                ReadIP_Button.IsEnabled = false;
                var data = HardPack.MakeReadPack(PackType.IP);
                Serial.Write(data, 0, 6);
                await Task.Run(() =>
                {
                    Thread.Sleep(1000);
                    if (Serial.BytesToRead > 0)
                    {
                        var temp = new byte[Serial.BytesToRead];
                        int len = Serial.BytesToRead - 6;
                        Serial.Read(temp, 0, Serial.BytesToRead);
                        var res = HardPack.CheckType(temp);
                        if (res == PackType.IP)
                        {
                            var temp1 = new byte[len];
                            Array.Copy(temp, 6, temp1, 0, len);
                            Dispatcher.Invoke(() => ReadIP(temp1));
                        }
                        App.ShowA("读信息", "地址已读取");
                    }
                    else
                    {
                        App.ShowB("读信息", "设备未响应");
                    }
                    Dispatcher.Invoke(() => ReadIP_Button.IsEnabled = true);
                    IsRead = false;
                });
            }
            else
            {
                App.ShowB("读信息", "设备未连接");
            }
        }

        private void ReadIP(byte[] data)
        {
            if (data.Length != 6)
            {
                App.ShowB("读信息", "数据包错误");
                return;
            }
            string temp = data[0].ToString();
            IP0.Text = temp;
            temp = data[1].ToString();
            IP1.Text = temp;
            temp = data[2].ToString();
            IP2.Text = temp;
            temp = data[3].ToString();
            IP3.Text = temp;
            Port0.Text = HardPack.ByteToInt(data[5], data[4]).ToString();
        }

        private void OnPaste1(object sender, DataObjectPastingEventArgs e)
        {
            if (IsLoad)
                return;
            var isText = e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true);
            if (!isText) return;

            var text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;
            e.CancelCommand();
            ToIPs(text);
            Port0.Focus();
        }
        private void OnPaste2(object sender, DataObjectPastingEventArgs e)
        {
            if (IsLoad)
                return;
            var isText = e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true);
            if (!isText) return;

            var text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;
            e.CancelCommand();
            ToUUIDs(text);
            UUID0.Focus();
        }

        private void ReadSensor_()
        {
            IsRead = true;
            Dispatcher.Invoke(() => ReadSensor_Button.IsEnabled = false);
            var data = HardPack.MakeReadPack(PackType.Sensor);
            Serial.Write(data, 0, 6);
            Thread.Sleep(1000);
            if (Serial.BytesToRead > 0)
            {
                var temp = new byte[Serial.BytesToRead];
                int len = Serial.BytesToRead - 6;
                Serial.Read(temp, 0, Serial.BytesToRead);
                var res = HardPack.CheckType(temp);
                if (res == PackType.Sensor)
                {
                    var temp1 = new byte[len];
                    Array.Copy(temp, 6, temp1, 0, len);
                    Dispatcher.Invoke(() => ReadSensor(temp1));
                }
                App.ShowA("读信息", "传感器已读取");
            }
            else
            {
                App.ShowB("读信息", "设备未响应");
            }
            Dispatcher.Invoke(() => ReadSensor_Button.IsEnabled = true);
            IsRead = false;
        }

        private async void ReadSensor_Click(object sender, RoutedEventArgs e)
        {
            if (IsRead)
                return;
            if (IsConnect)
            {
                IsRead = true;
                ReadSensor_Button.IsEnabled = false;
                var data = HardPack.MakeReadPack(PackType.Sensor);
                Serial.Write(data, 0, 6);
                await Task.Run(() =>
                {
                    Thread.Sleep(1000);
                    if (Serial.BytesToRead > 0)
                    {
                        var temp = new byte[Serial.BytesToRead];
                        int len = Serial.BytesToRead - 6;
                        Serial.Read(temp, 0, Serial.BytesToRead);
                        var res = HardPack.CheckType(temp);
                        if (res == PackType.Sensor)
                        {
                            var temp1 = new byte[len];
                            Array.Copy(temp, 6, temp1, 0, len);
                            Dispatcher.Invoke(() => ReadSensor(temp1));
                        }
                        App.ShowA("读信息", "传感器已读取");
                    }
                    else
                    {
                        App.ShowB("读信息", "设备未响应");
                    }
                    Dispatcher.Invoke(() => ReadSensor_Button.IsEnabled = true);
                    IsRead = false;
                });
            }
            else
            {
                App.ShowB("读信息", "设备未连接");
            }
        }

        private void ReadSensor(byte[] data)
        {
            if (data.Length != 8)
            {
                App.ShowB("读信息", "数据包错误");
                return;
            }
            int temp = HardPack.ByteToInt(data[0], data[1]);
            RADC.Text = temp.ToString();
            temp = HardPack.ByteToInt(data[3], data[2]);
            R1A.Text = temp.ToString();
            temp = HardPack.ByteToInt(data[6], data[5]);
            R2A.Text = temp.ToString();
            R1B.Text = data[4].ToString();
            R2B.Text = data[7].ToString();
        }

        private void SetSetting_Click(object sender, RoutedEventArgs e)
        {
            if (IsSend)
                return;
            if (!IsConnect)
            {
                App.ShowB("设置", "设备未连接");
                return;
            }
            if (!SetCheck())
            {
                App.ShowB("设置", "硬件配置填写错误");
                return;
            }
            IsSend = true;
            SetSetting_Button.IsEnabled = false;
            var temp = new byte[8];
            var temp2 = HardPack.IntToByte(int.Parse(WDis.Text));
            temp[0] = temp2[1];
            temp[1] = temp2[0];
            temp2 = HardPack.IntToByte(int.Parse(WADC2.Text));
            temp[2] = temp2[0];
            temp[3] = temp2[1];
            temp2 = HardPack.IntToByte(int.Parse(WADC1.Text));
            temp[4] = temp2[0];
            temp[5] = temp2[1];
            temp[6] = byte.Parse(WServo1.Text);
            temp[7] = byte.Parse(WServo2.Text);
            Task.Run(() =>
            {
                var data = HardPack.MakeSetPack(PackType.Set, temp);
                Serial.Write(data, 0, data.Length);
                Thread.Sleep(1000);
                if (Serial.BytesToRead <= 0)
                {
                    App.ShowB("设置", "设备未响应");
                    Dispatcher.Invoke(() => SetSetting_Button.IsEnabled = true);
                    IsSend = false;
                    return;
                }
                data = new byte[Serial.BytesToRead];
                Serial.Read(data, 0, Serial.BytesToRead);
                if (HardPack.CheckOK(data))
                {
                    App.ShowA("设置", "硬件配置已设置");
                }
                else
                {
                    App.ShowB("设置", "硬件配置设置失败");
                }
                Dispatcher.Invoke(() => SetSetting_Button.IsEnabled = true);
                IsSend = false;
            });
        }

        private bool SetCheck()
        {
            if (WADC1.Text.Length == 0 || WADC2.Text.Length == 0 || WServo1.Text.Length == 0 || WServo2.Text.Length == 0 || WDis.Text.Length == 0)
                return false;
            if (!CheckInputNumber(WADC1.Text) || !CheckInputNumber(WADC2.Text) || !CheckInputNumber(WServo1.Text) || !CheckInputNumber(WServo2.Text) || !CheckInputNumber(WDis.Text))
                return false;
            if (int.Parse(WADC1.Text) > 4096 || int.Parse(WADC2.Text) > 4086 || int.Parse(WServo1.Text) > 180 || int.Parse(WServo2.Text) > 180 || int.Parse(WDis.Text) > 8196)
                return false;
            return true;
        }

        private void ReadSetting_()
        {
            IsRead = true;
            Dispatcher.Invoke(() => ReadSetting_Button.IsEnabled = false);
            var data = HardPack.MakeReadPack(PackType.Set);
            Serial.Write(data, 0, 6);
            Thread.Sleep(1000);
            if (Serial.BytesToRead > 0)
            {
                var temp = new byte[Serial.BytesToRead];
                int len = Serial.BytesToRead - 6;
                Serial.Read(temp, 0, Serial.BytesToRead);
                var res = HardPack.CheckType(temp);
                if (res == PackType.Set)
                {
                    var temp1 = new byte[len];
                    Array.Copy(temp, 6, temp1, 0, len);
                    Dispatcher.Invoke(() => ReadSet(temp1));
                }
                App.ShowA("读信息", "硬件设置已读取");
            }
            else
            {
                App.ShowB("读信息", "设备未响应");
            }
            Dispatcher.Invoke(() => ReadSetting_Button.IsEnabled = true);
            IsRead = false;
        }

        private async void ReadSetting_Click(object sender, RoutedEventArgs e)
        {
            if (IsRead)
                return;
            if (IsConnect)
            {
                IsRead = true;
                ReadSetting_Button.IsEnabled = false;
                var data = HardPack.MakeReadPack(PackType.Set);
                Serial.Write(data, 0, 6);
                await Task.Run(() =>
                {
                    Thread.Sleep(1000);
                    if (Serial.BytesToRead > 0)
                    {
                        var temp = new byte[Serial.BytesToRead];
                        int len = Serial.BytesToRead - 6;
                        Serial.Read(temp, 0, Serial.BytesToRead);
                        var res = HardPack.CheckType(temp);
                        if (res == PackType.Set)
                        {
                            var temp1 = new byte[len];
                            Array.Copy(temp, 6, temp1, 0, len);
                            Dispatcher.Invoke(() => ReadSet(temp1));
                        }
                        App.ShowA("读信息", "硬件设置已读取");
                    }
                    else
                    {
                        App.ShowB("读信息", "设备未响应");
                    }
                    Dispatcher.Invoke(() => ReadSetting_Button.IsEnabled = true);
                    IsRead = false;
                });
            }
            else
            {
                App.ShowB("读信息", "设备未连接");
            }
        }

        private void ReadSet(byte[] data)
        {
            if (data.Length != 8)
            {
                App.ShowB("读信息", "数据包错误");
                return;
            }
            int temp = HardPack.ByteToInt(data[1], data[0]);
            WDis.Text = temp.ToString();
            temp = HardPack.ByteToInt(data[3], data[2]);
            WADC2.Text = temp.ToString();
            temp = HardPack.ByteToInt(data[5], data[4]);
            WADC1.Text = temp.ToString();
            WServo1.Text = data[6].ToString();
            WServo2.Text = data[7].ToString();
        }

        private void WADC_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoad)
                return;
            TextBox box = sender as TextBox;
            if (box == null)
                return;
            if (box.Text.Length > 4)
            {
                box.Text = "";
            }
            else
            {
                if (!CheckInputNumber(box.Text))
                {
                    box.Text = "";
                    return;
                }
                if (box.Text.Length == 0)
                    return;
                int temp = int.Parse(box.Text);
                if (temp > 4096)
                {
                    box.Text = "";
                }
            }
        }

        private void WServo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoad)
                return;
            TextBox box = sender as TextBox;
            if (box == null)
                return;
            if (box.Text.Length > 3)
            {
                box.Text = "";
            }
            else
            {
                if (!CheckInputNumber(box.Text))
                {
                    box.Text = "";
                    return;
                }
                if (box.Text.Length == 0)
                    return;
                int temp = int.Parse(box.Text);
                if (temp > 180)
                {
                    box.Text = "";
                }
            }
        }

        private void WDis_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoad)
                return;
            TextBox box = sender as TextBox;
            if (box == null)
                return;
            if (box.Text.Length > 4)
            {
                box.Text = "";
            }
            else
            {
                if (!CheckInputNumber(box.Text))
                {
                    box.Text = "";
                    return;
                }
                if (box.Text.Length == 0)
                    return;
                int temp = int.Parse(box.Text);
                if (temp > 8196)
                {
                    box.Text = "";
                }
            }
        }

        private void MQTT1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoad)
                return;
            TextBox box = sender as TextBox;
            if (box == null)
                return;
            if (box.Text.Length > 15)
            {
                box.Text = "";
            }
            else
            {
                if (!CheckInput(box.Text))
                {
                    box.Text = "";
                    return;
                }
            }
        }
        private bool MqttCheck()
        {
            if (MQTT1.Text.Length == 0 || MQTT1.Text.Length == 0)
                return false;
            if (!CheckInput(MQTT1.Text) || !CheckInputNumber(MQTT2.Text))
                return false;
            return true;
        }
        private void SetMqtt_Click(object sender, RoutedEventArgs e)
        {
            if (IsSend)
                return;
            if (!IsConnect)
            {
                App.ShowB("设置", "设备未连接");
                return;
            }
            if (!MqttCheck())
            {
                App.ShowB("设置", "MQTT设置填写错误");
                return;
            }
            IsSend = true;
            SetMqtt_Button.IsEnabled = false;
            var temp = new byte[32];
            var temp1 = Encoding.UTF8.GetBytes(MQTT1.Text);
            var temp2 = Encoding.UTF8.GetBytes(MQTT2.Text);
            Array.Copy(temp1, 0, temp, 0, Math.Min(16, temp1.Length));
            Array.Copy(temp2, 0, temp, 16, Math.Min(16, temp2.Length));
            Task.Run(() =>
            {
                var data = HardPack.MakeSetPack(PackType.Mqtt, temp);
                Serial.Write(data, 0, data.Length);
                Thread.Sleep(1000);
                if (Serial.BytesToRead <= 0)
                {
                    App.ShowB("设置", "设备未响应");
                    Dispatcher.Invoke(() => SetMqtt_Button.IsEnabled = true);
                    IsSend = false;
                    return;
                }
                data = new byte[Serial.BytesToRead];
                Serial.Read(data, 0, Serial.BytesToRead);
                if (HardPack.CheckOK(data))
                {
                    App.ShowA("设置", "MQTT设置已设置");
                }
                else
                {
                    App.ShowB("设置", "MQTT设置设置失败");
                }
                Dispatcher.Invoke(() => SetMqtt_Button.IsEnabled = true);
                IsSend = false;
            });
        }

        private void ReadMqtt_()
        {
            IsRead = true;
            Dispatcher.Invoke(() => ReadMqtt_Button.IsEnabled = false);
            var data = HardPack.MakeReadPack(PackType.Mqtt);
            Serial.Write(data, 0, 6);
            Thread.Sleep(1000);
            if (Serial.BytesToRead > 0)
            {
                var temp = new byte[Serial.BytesToRead];
                int len = Serial.BytesToRead - 6;
                Serial.Read(temp, 0, Serial.BytesToRead);
                var res = HardPack.CheckType(temp);
                if (res == PackType.Mqtt)
                {
                    var temp1 = new byte[len];
                    Array.Copy(temp, 6, temp1, 0, len);
                    Dispatcher.Invoke(() => ReadMqtt(temp1));
                }
                App.ShowA("读信息", "MQTT设置已读取");
            }
            else
            {
                App.ShowB("读信息", "设备未响应");
            }
            Dispatcher.Invoke(() => ReadMqtt_Button.IsEnabled = true);
            IsRead = false;
        }
        private async void ReadMqtt_Click(object sender, RoutedEventArgs e)
        {
            if (IsRead)
                return;
            if (IsConnect)
            {
                IsRead = true;
                ReadMqtt_Button.IsEnabled = false;
                var data = HardPack.MakeReadPack(PackType.Mqtt);
                Serial.Write(data, 0, 6);
                await Task.Run(() =>
                {
                    Thread.Sleep(1000);
                    if (Serial.BytesToRead > 0)
                    {
                        var temp = new byte[Serial.BytesToRead];
                        int len = Serial.BytesToRead - 6;
                        Serial.Read(temp, 0, Serial.BytesToRead);
                        var res = HardPack.CheckType(temp);
                        if (res == PackType.Mqtt)
                        {
                            var temp1 = new byte[len];
                            Array.Copy(temp, 6, temp1, 0, len);
                            Dispatcher.Invoke(() => ReadMqtt(temp1));
                        }
                        App.ShowA("读信息", "MQTT设置已读取");
                    }
                    else
                    {
                        App.ShowB("读信息", "设备未响应");
                    }
                    Dispatcher.Invoke(() => ReadMqtt_Button.IsEnabled = true);
                    IsRead = false;
                });
            }
            else
            {
                App.ShowB("读信息", "设备未连接");
            }
        }
        private void ReadMqtt(byte[] data)
        {
            if (data.Length != 32)
            {
                App.ShowB("读信息", "数据包错误");
                return;
            }
            for (int a = 0; a < 16; a++)
            {
                if (data[a] == 0)
                {
                    MQTT1.Text = Encoding.UTF8.GetString(data, 0, a);
                    break;
                }
            }
            for (int a = 0; a < 16; a++)
            {
                if (data[a + 16] == 0)
                {
                    MQTT2.Text = Encoding.UTF8.GetString(data, 16, a);
                    break;
                }
            }
        }
    }
}
