package com.coloryrtrash.app.ui;

import android.annotation.SuppressLint;
import android.location.Location;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import androidx.annotation.NonNull;
import androidx.appcompat.app.AlertDialog;
import androidx.fragment.app.Fragment;
import com.baidu.location.BDAbstractLocationListener;
import com.baidu.location.BDLocation;
import com.baidu.location.LocationClient;
import com.baidu.location.LocationClientOption;
import com.baidu.mapapi.map.*;
import com.baidu.mapapi.model.LatLng;
import com.coloryrtrash.app.GPSUtils;
import com.coloryrtrash.app.MainActivity;
import com.coloryrtrash.app.R;
import com.coloryrtrash.app.objs.TrashSaveObj;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Map;

public class MapFragment extends Fragment {

    @SuppressLint("StaticFieldLeak")
    private static MapView mMapView;

    @SuppressLint("StaticFieldLeak")
    private static View root;

    private static BaiduMap map;
    private static final Map<String, Overlay> points = new HashMap<>();
    private LocationClient mLocationClient;

    private final String key = "info";

    static class MyLocationListener implements GPSUtils.OnLocationResultListener {
        @Override
        public void onLocationResult(Location location) {
            if (location == null || mMapView == null) {
                return;
            }
            MyLocationData locData = new MyLocationData.Builder()
                    .accuracy(location.getAccuracy())
                    .direction(location.getBearing())
                    .latitude(location.getLatitude())
                    .longitude(location.getLongitude()).build();
            map.setMyLocationData(locData);
        }

        @Override
        public void OnLocationChange(Location location) {
            if (location == null || mMapView == null) {
                return;
            }
            MyLocationData locData = new MyLocationData.Builder()
                    .accuracy(location.getAccuracy())
                    .direction(location.getBearing())
                    .latitude(location.getLatitude())
                    .longitude(location.getLongitude()).build();
            map.setMyLocationData(locData);
        }
    }

    public View onCreateView(@NonNull LayoutInflater inflater,
                             ViewGroup container, Bundle savedInstanceState) {
        if (root == null)
            root = inflater.inflate(R.layout.fragment_map, container, false);
        mMapView = root.findViewById(R.id.bmapView);
        map = mMapView.getMap();
        map.setMyLocationEnabled(true);
        map.setCompassEnable(true);

        MainActivity.GPSUtils.getLngAndLat(new MyLocationListener());

        map.setOnMarkerClickListener(marker -> {
            Bundle bundle = marker.getExtraInfo();
            String temp = bundle.getString(key);
            AlertDialog.Builder builder = new AlertDialog.Builder(root.getContext());
            AlertDialog alert = builder.setMessage(temp).create();
            alert.show();                    //显示对话框
            return true;
        });
        return root;
    }

    @Override
    public void onResume() {
        super.onResume();
        mMapView.onResume();
    }

    @Override
    public void onPause() {
        super.onPause();
        mMapView.onPause();
    }

    @Override
    public void onDestroy() {
        super.onDestroy();
        mLocationClient.stop();
        map.setMyLocationEnabled(false);
        mMapView.onDestroy();
        mMapView = null;
    }

    private String getString(TrashSaveObj item) {
        return "别称:" + item.Nick + "\n坐标:" + item.X + ", " + item.Y +
                "\n容量:" + item.Capacity + "\n是否打开:" + item.Open +
                "\n状态:" + item.State + "\n上线时间:" + item.Time +
                "\nSIM卡号:" + item.SIM + "\n电量:" + item.Battery;
    }

    public void addTrash(TrashSaveObj item) {
        LatLng point = new LatLng(item.Y, item.X);
        BitmapDescriptor bitmap = BitmapDescriptorFactory.fromResource(R.drawable.icon);
        Bundle Bundle = new Bundle();
        Bundle.putString(key, getString(item));
        OverlayOptions option = new MarkerOptions()
                .position(point)
                .icon(bitmap)
                .scaleX(0.5f)
                .scaleY(0.5f)
                .clickable(true)
                .extraInfo(Bundle);
        Overlay overlay = map.addOverlay(option);
        points.put(item.UUID, overlay);
        MapStatus mMapStatus = new MapStatus.Builder()
                .target(point)
                .zoom(18)
                .build();
        MapStatusUpdate mMapStatusUpdate = MapStatusUpdateFactory.newMapStatus(mMapStatus);
        map.setMapStatus(mMapStatusUpdate);
    }

    public void remove(String uuid) {

    }

    public void clear() {
        map.removeOverLays(new ArrayList<>(points.values()));
    }

}