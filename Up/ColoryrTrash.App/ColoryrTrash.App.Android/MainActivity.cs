using Android.App;
using Android.App.Job;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Security;
using System.Threading.Tasks;

namespace ColoryrTrash.App.Droid
{
    [Activity(Label = "ColoryrTrash.App", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static MainActivity MainActivity_;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            MainActivity_ = this;
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            //ComponentName componentName = new ComponentName(this, new Self().Class);
            //JobInfo jobInfo = new JobInfo.Builder(12, componentName)
            //        .SetRequiredNetworkType(NetworkType.Any)
            //        .Build();
            //JobScheduler jobScheduler = (JobScheduler)GetSystemService(JOB_SCHEDULER_SERVICE);
            //int resultCode = jobScheduler.schedule(jobInfo);
            //if (resultCode == JobScheduler.RESULT_SUCCESS)
            //{
            //    Console.WriteLine(TAG, "Job scheduled!");
            //}
            //else
            //{
            //    Console.WriteLine(TAG, "Job not scheduled");
            //}

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }
        /// <summary>
        /// 重写按键事件
        /// </summary>
        /// <param name="keyCode"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            // 根据某种情形禁用返回键
            if (keyCode == Keycode.Back)
            {
                PackageManager pm = PackageManager;
                ResolveInfo homeInfo = pm.ResolveActivity(new Intent(Intent.ActionMain).AddCategory(Intent.CategoryHome), 0);
                ActivityInfo ai = homeInfo.ActivityInfo;
                Intent startIntent = new Intent(Intent.ActionMain);
                startIntent.AddCategory(Intent.CategoryLauncher);
                startIntent.SetComponent(new ComponentName(ai.PackageName, ai.Name));
                StartActivitySafely(startIntent);
                return false;
            }

            return base.OnKeyDown(keyCode, e);
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        private void StartActivitySafely(Intent intent)
        {
            intent.AddFlags(ActivityFlags.NewTask);
            try
            {
                StartActivity(intent);
            }
            catch (ActivityNotFoundException ex)
            {
                Toast.MakeText(this, "StartActivitySafely()异常:" + ex.Message, ToastLength.Short).Show();
            }
            catch (SecurityException ex)
            {
                Toast.MakeText(this, "StartActivitySafely()异常:" + ex.Message, ToastLength.Short).Show();
            }
        }
    }
}