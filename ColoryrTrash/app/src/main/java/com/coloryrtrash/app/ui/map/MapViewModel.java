package com.coloryrtrash.app.ui.map;

import androidx.lifecycle.LiveData;
import androidx.lifecycle.MutableLiveData;
import androidx.lifecycle.ViewModel;
import com.baidu.mapapi.map.MapView;

public class MapViewModel extends ViewModel {

    private MutableLiveData<String> mText;
    private MapView mMapView = null;

    public MapViewModel() {
        mText = new MutableLiveData<>();
        mText.setValue("This is slideshow fragment");
    }

    public LiveData<String> getText() {
        return mText;
    }

    public void addPoint()
    {

    }
}