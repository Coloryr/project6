using ColoryrTrash.App.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColoryrTrash.App
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : FlyoutPage
    {
        private PageName NowSel;
        public MainPage()
        {
            InitializeComponent();
            flyoutPage.listView.ItemSelected += OnItemSelected;
        }

        private void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as FlyoutPageItem;
            if (item != null)
            {
                if (NowSel != item.Name)
                    Switch(item.Name);
            }
            flyoutPage.listView.SelectedItem = null;
            IsPresented = false;
        }
        public void Switch(PageName name)
        {
            switch (name)
            {
                case PageName.MainPage:
                    Detail = App.helloPage_;
                    break;
                case PageName.InfoPage:
                    Detail = App.infoPage_;
                    break;
                case PageName.LoginPage:
                    Detail = App.loginPage_;
                    break;
                case PageName.MapPage:
                    Detail = App.mapPage_;
                    break;
            }
            NowSel = name;
        }

        internal void SetName(string v)
        {
            flyoutPage.SetName(v);
        }

        public void ClearName()
        {
            flyoutPage.ClearName();
        }
    }
}