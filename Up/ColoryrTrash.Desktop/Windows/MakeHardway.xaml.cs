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
using System.Windows.Input;
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
        public MakeHardway()
        {
            InitializeComponent();
            Serial = new();
            BaudRates = new int[] { 4800, 9600, 19200, 38400, 115200 };
            ComList.ItemsSource = SerialPort.GetPortNames();
            if (SerialPort.GetPortNames().Length > 0)
            {
                ComList.SelectedItem = SerialPort.GetPortNames()[0];
            }
            DataContext = this;
            BaudRate.SelectedItem = 115200;
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            App.MakeHardway_ = null;
        }
        private bool CheckInput(string data)
        {
            return Regex.IsMatch(data, "^[A-Za-z0-9]+$");
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
                    Group1.IsEnabled = true;
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
                Group1.IsEnabled = false;
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
                        State_Led.Fill = Brushes.LawnGreen;
                    });
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
                index ++;
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
                temp += UUID15 .Text;
            else
                return null;
            return temp;
        }

        private void UUID0_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox box = sender as TextBox;
            if (box == null)
                return;
            if (box.Text.Length > 1)
            {
                ToUUIDs(box.Text);
            }
            else if (box.Text.Length == 1)
            {
                if (!CheckInput(box.Text))
                {
                    box.Text = "";
                }
            }
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
            App.MqttUtils.CheckUUID(temp);
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
                Thread.Sleep(100);
                if (Serial.BytesToRead <= 0)
                {
                    App.ShowB("设置", "UUID设置失败");
                    IsSend = false;
                    return;
                }
                data = new byte[Serial.BytesToRead];
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
    }
}
