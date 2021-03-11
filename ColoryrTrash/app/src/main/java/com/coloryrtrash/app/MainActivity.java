package com.coloryrtrash.app;

import android.os.Bundle;
import android.view.MenuItem;
import androidx.appcompat.app.ActionBarDrawerToggle;
import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.widget.Toolbar;
import androidx.drawerlayout.widget.DrawerLayout;
import androidx.fragment.app.FragmentManager;
import androidx.fragment.app.FragmentTransaction;
import com.baidu.mapapi.CoordType;
import com.baidu.mapapi.SDKInitializer;
import com.coloryrtrash.app.ui.HomeFragment;
import com.coloryrtrash.app.ui.ListFragment;
import com.coloryrtrash.app.ui.MapFragment;
import com.coloryrtrash.app.ui.UserFragment;
import com.google.android.material.navigation.NavigationView;

public class MainActivity extends AppCompatActivity {

    private FragmentManager fManager;

    private HomeFragment HomeFragment;
    private ListFragment ListFragment;
    private MapFragment MapFragment;
    private UserFragment UserFragment;

    private ActionBarDrawerToggle mDrawerToggle;
    private DrawerLayout mDrawerLayout;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        SDKInitializer.initialize(this.getApplication());
        SDKInitializer.setCoordType(CoordType.BD09LL);

        setContentView(R.layout.activity_main);
        fManager = getSupportFragmentManager();
        NavigationView navigationView = findViewById(R.id.nav_view);

        final Toolbar toolbar = findViewById(R.id.toolbar);
        toolbar.setNavigationIcon(R.drawable.ic_menu);
        setSupportActionBar(toolbar);

        mDrawerLayout = findViewById(R.id.drawer_layout);
        mDrawerToggle = new ActionBarDrawerToggle(this, mDrawerLayout, 0,0);
        mDrawerLayout.addDrawerListener(mDrawerToggle);

        navigationView.setNavigationItemSelectedListener(menuItem -> {
            FragmentTransaction fTransaction = fManager.beginTransaction();
            hideAllFragment(fTransaction);
            switch (menuItem.getItemId()) {
                case R.id.nav_home:
                    toolbar.setTitle(R.string.menu_home);
                    if (HomeFragment == null) {
                        HomeFragment = new HomeFragment();
                        fTransaction.add(R.id.nav_host_fragment, HomeFragment);
                    } else {
                        fTransaction.show(HomeFragment);
                    }
                    break;
                case R.id.nav_list:
                    toolbar.setTitle(R.string.menu_list);
                    if (ListFragment == null) {
                        ListFragment = new ListFragment();
                        fTransaction.add(R.id.nav_host_fragment, ListFragment);
                    } else {
                        fTransaction.show(ListFragment);
                    }
                    break;
                case R.id.nav_user:
                    toolbar.setTitle(R.string.menu_user);
                    if (UserFragment == null) {
                        UserFragment = new UserFragment();
                        fTransaction.add(R.id.nav_host_fragment, UserFragment);
                    } else {
                        fTransaction.show(UserFragment);
                    }
                    break;
                case R.id.nav_map:
                    toolbar.setTitle(R.string.menu_map);
                    if (MapFragment == null) {
                        MapFragment = new MapFragment();
                        fTransaction.add(R.id.nav_host_fragment, MapFragment);
                    } else {
                        fTransaction.show(MapFragment);
                    }
                    break;
            }
            mDrawerLayout.closeDrawer(navigationView);
            fTransaction.commit();
            return true;
        });

        navigationView.setCheckedItem(R.id.nav_home);
        HomeFragment = new HomeFragment();
        FragmentTransaction fTransaction = fManager.beginTransaction();
        fTransaction.add(R.id.nav_host_fragment, HomeFragment);
        fTransaction.commit();
    }

    //覆写方法让系统判断点击的图标后是否弹出侧拉页面
    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        mDrawerToggle.onOptionsItemSelected(item);
        return super.onOptionsItemSelected(item);
    }

    //隐藏所有Fragment
    private void hideAllFragment(FragmentTransaction fragmentTransaction) {
        if (HomeFragment != null) fragmentTransaction.hide(HomeFragment);
        if (ListFragment != null) fragmentTransaction.hide(ListFragment);
        if (MapFragment != null) fragmentTransaction.hide(MapFragment);
        if (UserFragment != null) fragmentTransaction.hide(UserFragment);
    }
}