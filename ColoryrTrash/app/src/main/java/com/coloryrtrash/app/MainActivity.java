package com.coloryrtrash.app;

import android.Manifest;
import android.annotation.SuppressLint;
import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.os.Bundle;
import android.view.MenuItem;
import android.widget.TextView;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.appcompat.app.ActionBarDrawerToggle;
import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.widget.Toolbar;
import androidx.core.app.ActivityCompat;
import androidx.drawerlayout.widget.DrawerLayout;
import androidx.fragment.app.FragmentManager;
import androidx.fragment.app.FragmentTransaction;
import com.alibaba.fastjson.JSON;
import com.baidu.mapapi.CoordType;
import com.baidu.mapapi.SDKInitializer;
import com.coloryrtrash.app.objs.ConfigObj;
import com.coloryrtrash.app.objs.TrashSaveObj;
import com.coloryrtrash.app.ui.HomeFragment;
import com.coloryrtrash.app.ui.MapFragment;
import com.coloryrtrash.app.ui.TrashFragment;
import com.coloryrtrash.app.ui.UserFragment;
import com.google.android.material.navigation.NavigationView;

public class MainActivity extends AppCompatActivity {

    public static ConfigObj config;
    public static final String fileName = "config.json";

    @SuppressLint("StaticFieldLeak")
    public static MainActivity MainActivity;
    @SuppressLint("StaticFieldLeak")
    private static FileUtils FileUtils;

    private FragmentManager fManager;

    private HomeFragment homeFragment;
    private TrashFragment trashFragment;
    private MapFragment mapFragment;
    private UserFragment userFragment;

    private ActionBarDrawerToggle mDrawerToggle;
    private DrawerLayout mDrawerLayout;

    private Toolbar toolbar;
    private NavigationView navigationView;

    private static Intent intent;

    public static boolean isRun;
    public static boolean isLogin;
    public static String pass;
    public static String groupName;

    @SuppressLint("StaticFieldLeak")
    public static GPSUtils GPSUtils;

    private static NotificationManager mNManager;

    @SuppressLint("StaticFieldLeak")
    private static TextView userName;

    public static void full() {
        if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.O) {
            PendingIntent pendingIntent = PendingIntent.getActivity(MainActivity.getApplicationContext(),
                    1, intent, PendingIntent.FLAG_CANCEL_CURRENT);
            Notification.Builder mBuilder = new Notification.Builder(MainActivity.getApplicationContext(),
                    "ColoryrTrash");
            mBuilder.setContentTitle("垃圾桶快满")
                    .setContentText("有垃圾桶快满~")
                    .setTicker("有垃圾桶快满~")
                    .setSmallIcon(R.mipmap.icon)
                    .setContentIntent(pendingIntent)
                    .setAutoCancel(true);
            Notification b = mBuilder.build();
            mNManager.notify(1, b);
        }
    }

    @Override
    public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults) {
        switch (requestCode) {
            case PackageManager.PERMISSION_GRANTED: {
                if (grantResults.length > 0
                        && grantResults[0] == PackageManager.PERMISSION_GRANTED
                        && grantResults[1] == PackageManager.PERMISSION_GRANTED) {
                    // 权限被用户同意。
                    // 执形我们想要的操作
                } else {
                    // 权限被用户拒绝了。
                    //若是点击了拒绝和不再提醒
                    //关于shouldShowRequestPermissionRationale
                    // 1、当用户第一次被询问是否同意授权的时候，返回false
                    // 2、当之前用户被询问是否授权，点击了false,并且点击了不在询问（第一次询问不会出现“不再询问”的选项），
                    // 之后便会返回false
                    // 3、当用户被关闭了app的权限，该app不允许授权的时候，返回false
                    // 4、当用户上一次不同意授权，没有点击“不再询问”的时候，下一次返回true
                    if (!ActivityCompat.shouldShowRequestPermissionRationale(this, Manifest.permission.ACCESS_COARSE_LOCATION)
                            || !ActivityCompat.shouldShowRequestPermissionRationale(this, Manifest.permission.ACCESS_COARSE_LOCATION)) {
                        //提示用户前往设置界面自己打开权限
                        Toast.makeText(this, "请前往设置界面打开权限", Toast.LENGTH_SHORT).show();
                        return;
                    }

                }
            }
        }
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        MainActivity = this;
        super.onCreate(savedInstanceState);

        GPSUtils = new GPSUtils(this);

        SDKInitializer.initialize(this.getApplication());
        SDKInitializer.setCoordType(CoordType.BD09LL);

        mNManager = (NotificationManager) this.getApplicationContext().getSystemService(NOTIFICATION_SERVICE);
        NotificationChannel channel = new NotificationChannel("ColoryrTrash", "ColoryrTrash", NotificationManager.IMPORTANCE_DEFAULT);
        channel.setDescription("ColoryrTrash");
        mNManager.createNotificationChannel(channel);

        setContentView(R.layout.activity_main);
        fManager = getSupportFragmentManager();
        navigationView = findViewById(R.id.nav_view);

        toolbar = findViewById(R.id.toolbar);
        toolbar.setNavigationIcon(R.drawable.ic_menu);
        setSupportActionBar(toolbar);

        mDrawerLayout = findViewById(R.id.drawer_layout);
        mDrawerToggle = new ActionBarDrawerToggle(this, mDrawerLayout, 0, 0);
        mDrawerLayout.addDrawerListener(mDrawerToggle);

        FragmentTransaction Transaction = fManager.beginTransaction();
        homeFragment = new HomeFragment();
        trashFragment = new TrashFragment();
        userFragment = new UserFragment();
        mapFragment = new MapFragment();
        Transaction.add(R.id.nav_fragment, homeFragment);
        Transaction.add(R.id.nav_fragment, trashFragment);
        Transaction.add(R.id.nav_fragment, userFragment);
        Transaction.add(R.id.nav_fragment, mapFragment);
        hideAllFragment(Transaction);
        navigationView.setCheckedItem(R.id.nav_home);
        userName = navigationView.getHeaderView(0).findViewById(R.id.userName);
        Transaction.show(homeFragment);
        Transaction.commit();
        navigationView.setNavigationItemSelectedListener(this::onNavigationItemSelected);

        FileUtils = new FileUtils(this.getApplicationContext());

        String temp = FileUtils.read(fileName);
        if (!temp.isEmpty()) {
            config = JSON.parseObject(temp, ConfigObj.class);
        }
        if (config == null) {
            config = new ConfigObj();
            config.ip = "127.0.0.1";
            config.port = 12345;
            config.user = "user";
            save();
        }

        intent = new Intent(this, MqttUtils.class);

        MqttUtils.token = config.token;

        if (config.auto) {
            start("");
        }
    }

    public static void local(TrashSaveObj item) {
        MainActivity.mapFragment.clear();
        MainActivity.mapFragment.addTrash(item);
        MainActivity.move(R.id.nav_map);
    }

    public static void loginDone() {
        isLogin = true;
        save();
        HomeFragment.setUser(config.user);
        userName.setText(config.user);
        MainActivity.move(R.id.nav_home);
        MainActivity.userFragment.close();
        MqttUtils.getInfo();
        MqttUtils.getItems();
    }

    public static void isConnect() {
        if (config.auto && !config.token.isEmpty()) {
            MqttUtils.checkLogin();
        } else if (pass != null) {
            MqttUtils.Login();
        }
    }

    public static void loginOut() {
        isLogin = false;
        config.token = "";
        save();
        userName.setText(R.string.user_no_login);
        HomeFragment.clear();
        MainActivity.move(R.id.nav_user);
        MainActivity.userFragment.open();
    }

    public static void start(String temp) {
        if (isRun)
            stop();
        MqttUtils.start();
        pass = temp;
    }

    public static void stop() {
        MqttUtils.stop();
    }

    public static void save() {
        String temp = JSON.toJSONString(config);
        FileUtils.save(fileName, temp);
    }

    private boolean onNavigationItemSelected(@NonNull MenuItem menuItem) {
        move(menuItem.getItemId());
        mDrawerLayout.closeDrawer(navigationView);
        return true;
    }

    private void move(int id) {
        FragmentTransaction fTransaction = fManager.beginTransaction();
        hideAllFragment(fTransaction);
        switch (id) {
            case R.id.nav_home:
                toolbar.setTitle(R.string.menu_home);
                fTransaction.show(homeFragment);
                navigationView.setCheckedItem(R.id.nav_home);
                break;
            case R.id.nav_list:
                toolbar.setTitle(R.string.menu_list);
                fTransaction.show(trashFragment);
                navigationView.setCheckedItem(R.id.nav_list);
                break;
            case R.id.nav_user:
                toolbar.setTitle(R.string.menu_user);
                fTransaction.show(userFragment);
                navigationView.setCheckedItem(R.id.nav_user);
                break;
            case R.id.nav_map:
                toolbar.setTitle(R.string.menu_map);
                fTransaction.show(mapFragment);
                navigationView.setCheckedItem(R.id.nav_map);
                break;
        }
        fTransaction.commit();
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        mDrawerToggle.onOptionsItemSelected(item);
        return super.onOptionsItemSelected(item);
    }

    private void hideAllFragment(FragmentTransaction fragmentTransaction) {
        fragmentTransaction.hide(homeFragment);
        fragmentTransaction.hide(trashFragment);
        fragmentTransaction.hide(mapFragment);
        fragmentTransaction.hide(userFragment);
    }
}