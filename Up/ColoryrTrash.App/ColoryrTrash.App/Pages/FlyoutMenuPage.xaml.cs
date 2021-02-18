
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColoryrTrash.App.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FlyoutMenuPage : ContentPage
    {
        public FlyoutMenuPage()
        {
            InitializeComponent();
        }

        internal void SetName(string v)
        {
            Dispatcher.BeginInvokeOnMainThread(() =>
            {
                UserName.Text = $"欢迎回来 {v}";
            });
        }

        public void ClearName()
        {
            Dispatcher.BeginInvokeOnMainThread(() =>
            {
                UserName.Text = "未登录";
            });
        }
    }
}