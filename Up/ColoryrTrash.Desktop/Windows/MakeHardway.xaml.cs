using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
        public MakeHardway()
        {
            InitializeComponent();
            Serial = new();
            BaudRates = new int[] { 4800, 9600, 115200, 19200, 38400 };
            ComList.ItemsSource = SerialPort.GetPortNames();
            DataContext = this;
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
                State_Led.Fill = Brushes.Red;
                Open = false;
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            ComList.ItemsSource = SerialPort.GetPortNames();
        }

        private void TestConnect()
        {
            Serial.Write(HardPack.TestPack, 0, 9);
        }
    }
}
