package com.coloryrtrash.app;

import android.annotation.SuppressLint;
import android.os.Bundle;
import android.view.MenuItem;
import androidx.annotation.NonNull;
import androidx.appcompat.app.ActionBarDrawerToggle;
import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.widget.Toolbar;
import androidx.drawerlayout.widget.DrawerLayout;
import androidx.fragment.app.FragmentManager;
import androidx.fragment.app.FragmentTransaction;
import com.alibaba.fastjson.JSON;
import com.baidu.mapapi.CoordType;
import com.baidu.mapapi.SDKInitializer;
import com.coloryrtrash.app.objs.ConfigObj;
import com.coloryrtrash.app.ui.HomeFragment;
import com.coloryrtrash.app.ui.TrashFragment;
import com.coloryrtrash.app.ui.MapFragment;
import com.coloryrtrash.app.ui.UserFragment;
import com.google.android.material.navigation.NavigationView;

public class MainActivity extends AppCompatActivity {

    public static ConfigObj config;
    public static final String fileName = "config.json";

    @SuppressLint("StaticFieldLeak")
    private static FileUtils FileUtils;

    private FragmentManager fManager;

    private HomeFragment HomeFragment;
    private TrashFragment TrashFragment;
    private MapFragment MapFragment;
    private UserFragment UserFragment;

    private ActionBarDrawerToggle mDrawerToggle;
    private DrawerLayout mDrawerLayout;

    private Toolbar toolbar;
    private NavigationView navigationView;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        SDKInitializer.initialize(this.getApplication());
        SDKInitializer.setCoordType(CoordType.BD09LL);

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
        HomeFragment = new HomeFragment();
        TrashFragment = new TrashFragment();
        UserFragment = new UserFragment();
        MapFragment = new MapFragment();
        Transaction.add(R.id.nav_fragment, HomeFragment);
        Transaction.add(R.id.nav_fragment, TrashFragment);
        Transaction.add(R.id.nav_fragment, UserFragment);
        Transaction.add(R.id.nav_fragment, MapFragment);
        hideAllFragment(Transaction);
        navigationView.setCheckedItem(R.id.nav_home);
        Transaction.show(HomeFragment);
        Transaction.commit();
        navigationView.setNavigationItemSelectedListener(this::onNavigationItemSelected);

        FileUtils = new FileUtils(this.getApplicationContext());

        String temp = FileUtils.read(fileName);
        if (!temp.isEmpty()) {
            config = JSON.parseObject(temp, ConfigObj.class);
        }
        if(config == null)
        {
            config = new ConfigObj();
            config.ip = "127.0.0.1";
            config.port = 12345;
            config.user = "user";
            save();
        }
    }

    public static void save() {
        String temp = JSON.toJSONString(config);
        FileUtils.save(fileName, temp);
    }

    private boolean onNavigationItemSelected(@NonNull MenuItem menuItem) {
        FragmentTransaction fTransaction = fManager.beginTransaction();
        hideAllFragment(fTransaction);
        switch (menuItem.getItemId()) {
            case R.id.nav_home:
                toolbar.setTitle(R.string.menu_home);
                fTransaction.show(HomeFragment);
                break;
            case R.id.nav_list:
                toolbar.setTitle(R.string.menu_list);
                fTransaction.show(TrashFragment);
                break;
            case R.id.nav_user:
                toolbar.setTitle(R.string.menu_user);
                fTransaction.show(UserFragment);
                break;
            case R.id.nav_map:
                toolbar.setTitle(R.string.menu_map);
                fTransaction.show(MapFragment);
                break;
        }
        mDrawerLayout.closeDrawer(navigationView);
        fTransaction.commit();
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        mDrawerToggle.onOptionsItemSelected(item);
        return super.onOptionsItemSelected(item);
    }

    //隐藏所有Fragment
    private void hideAllFragment(FragmentTransaction fragmentTransaction) {
        fragmentTransaction.hide(HomeFragment);
        fragmentTransaction.hide(TrashFragment);
        fragmentTransaction.hide(MapFragment);
        fragmentTransaction.hide(UserFragment);
    }
}