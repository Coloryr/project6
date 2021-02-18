
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColoryrTrash.App.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HelloPage : ContentPage
    {
        public HelloPage()
        {
            InitializeComponent();
        }

        public void SetName(string name)
        {
            Dispatcher.BeginInvokeOnMainThread(() =>
            {
                User.Text = name;
            });
        }

        public void ClearName()
        {
            Dispatcher.BeginInvokeOnMainThread(() =>
            {
                User.Text = "未登录";
            });
        }

        public void SetGroup(string name)
        {
            Dispatcher.BeginInvokeOnMainThread(() =>
            {
                Group.Text = name;
            });
        }
        public void ClearGroup()
        {
            Dispatcher.BeginInvokeOnMainThread(() =>
            {
                Group.Text = "组";
            });
        }
    }
}