using Android.App;
using Android.App.Job;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace ColoryrTrash.App.Droid
{
    [Service(Name = "ColoryrTrash.Servive", Permission="android.permission.BIND_JOB_SERVICE")]
    public class TestJob : JobService
    {
        private Thread thread;
        private bool IsRun;
        
        private void Task()
        { 
            while(IsRun)
            {
                try
                {
                    if (!MqttUtils.Client.IsConnected)
                    {
                        if (App.IsLogin && App.Config.AutoLogin)
                        {
                            if (!MqttUtils.Start().Result)
                            {
                                App.Config.AutoLogin = false;
                                return;
                            }
                        }
                    }
                    Thread.Sleep(1000);
                }
                catch (Exception e)
                { 
                    
                }
            }
        }
        public override bool OnStartJob(JobParameters jobParams)
        {
            thread = new Thread(Task);
            IsRun = true;
            thread.Start();
            return true;
        }

        public override bool OnStopJob(JobParameters jobParams)
        {
            IsRun = false;
            return false;
        }
    }
    [Activity(Label = "ColoryrTrash.App", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize
    | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static MainActivity MainActivity_;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            MainActivity_ = this;
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Java.Lang.Class javaClass = Java.Lang.Class.FromType(typeof(TestJob));
            ComponentName component = new ComponentName(this, javaClass);

            JobInfo.Builder builder = new JobInfo.Builder(1, component)
                                                 .SetMinimumLatency(1000)
                                                 .SetOverrideDeadline(5000)
                                                 .SetRequiredNetworkType(NetworkType.Unmetered);
            JobInfo jobInfo = builder.Build();

            JobScheduler jobScheduler = (JobScheduler)GetSystemService(JobSchedulerService);
            int result = jobScheduler.Schedule(jobInfo);
            if (result == JobScheduler.ResultSuccess)
            {

            }
            else
            {
                // Couldn't schedule the job.
            }

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