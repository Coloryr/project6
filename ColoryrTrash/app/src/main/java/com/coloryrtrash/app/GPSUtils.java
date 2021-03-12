package com.coloryrtrash.app;

import android.Manifest;
import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.Context;
import android.content.pm.PackageManager;
import android.location.Location;
import android.location.LocationListener;
import android.location.LocationManager;
import android.widget.Toast;
import androidx.core.app.ActivityCompat;

import java.util.List;

public class GPSUtils {
    @SuppressLint("StaticFieldLeak")
    private final Context mContext;
    private final Activity activity;
    private LocationManager locationManager;
    private String locationProvider;

    public GPSUtils(Activity activity) {
        this.activity = activity;
        this.mContext = activity.getApplication().getApplicationContext();
    }

    private void init() {
        locationManager = (LocationManager) mContext.getSystemService(Context.LOCATION_SERVICE);
        //获取所有可用的位置提供器
        List<String> providers = locationManager.getProviders(true);

        if (providers.contains(LocationManager.GPS_PROVIDER)) {
            //如果是GPS
            locationProvider = LocationManager.GPS_PROVIDER;
        } else if (providers.contains(LocationManager.NETWORK_PROVIDER)) {
            //如果是Network
            locationProvider = LocationManager.NETWORK_PROVIDER;
        }

        if (ActivityCompat.checkSelfPermission(mContext, Manifest.permission.ACCESS_FINE_LOCATION) != PackageManager.PERMISSION_GRANTED
                && ActivityCompat.checkSelfPermission(mContext, Manifest.permission.ACCESS_COARSE_LOCATION) != PackageManager.PERMISSION_GRANTED) {
            ActivityCompat.requestPermissions(activity, new String[]{Manifest.permission.ACCESS_FINE_LOCATION,
                    Manifest.permission.ACCESS_COARSE_LOCATION}, PackageManager.PERMISSION_GRANTED);
            return;
        }
        if (locationProvider == null) {
            Toast.makeText(mContext, "GPS定位没有开启", Toast.LENGTH_SHORT).show();
            return;
        }
        locationManager.requestLocationUpdates(locationProvider, 3000, 1, locationListener);
    }

    public void setLocationListener(OnLocationResultListener onLocationResultListener) {
        mOnLocationListener = onLocationResultListener;
    }

    /**
     * 获取经纬度
     */
    public Location getLngAndLat() {
        if (locationManager == null || locationProvider == null) {
            init();
            return null;
        }
        if (ActivityCompat.checkSelfPermission(mContext, Manifest.permission.ACCESS_FINE_LOCATION) != PackageManager.PERMISSION_GRANTED
                && ActivityCompat.checkSelfPermission(mContext, Manifest.permission.ACCESS_COARSE_LOCATION) != PackageManager.PERMISSION_GRANTED) {
            ActivityCompat.requestPermissions(activity, new String[]{Manifest.permission.ACCESS_FINE_LOCATION,
                    Manifest.permission.ACCESS_COARSE_LOCATION}, PackageManager.PERMISSION_GRANTED);
            return null;
        }
        return locationManager.getLastKnownLocation(locationProvider);
    }


    public LocationListener locationListener = new LocationListener() {
        //当坐标改变时触发此函数，如果Provider传进相同的坐标，它就不会被触发
        @Override
        public void onLocationChanged(Location location) {
            if (mOnLocationListener != null) {
                mOnLocationListener.OnLocationChange(location);
            }
        }
    };

    public void removeListener() {
        locationManager.removeUpdates(locationListener);
    }

    private OnLocationResultListener mOnLocationListener;

    public interface OnLocationResultListener {
        void OnLocationChange(Location location);
    }
}