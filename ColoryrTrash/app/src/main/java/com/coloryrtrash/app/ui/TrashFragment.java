package com.coloryrtrash.app.ui;

import android.annotation.SuppressLint;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.fragment.app.Fragment;
import com.coloryrtrash.app.*;
import com.coloryrtrash.app.objs.TrashSaveObj;

import java.util.Collections;
import java.util.LinkedList;
import java.util.List;

public class TrashFragment extends Fragment {

    @SuppressLint("StaticFieldLeak")
    private static PullRefresh mySelfListView;

    @SuppressLint("StaticFieldLeak")
    private static View root;

    private final static List<TrashSaveObj> mData = new LinkedList<>();
    @SuppressLint("StaticFieldLeak")
    private static ItemAdapter mAdapter = null;

    public static void setList(List<TrashSaveObj> list) {
        mData.clear();
        mData.addAll(list);
        Collections.sort(mData); // 按年龄排序
        Handler mainHandler = new Handler(Looper.getMainLooper());
        mainHandler.post(() -> {
            mAdapter.notifyDataSetChanged();
            mySelfListView.updateHeaderResult();
        });
    }

    public static void setDone() {
        mySelfListView.updateHeaderResult();
    }

    public View onCreateView(@NonNull LayoutInflater inflater,
                             ViewGroup container, Bundle savedInstanceState) {
        if (root == null)
            root = inflater.inflate(R.layout.fragment_list, container, false);
        mySelfListView = root.findViewById(R.id.listview);
        mAdapter = new ItemAdapter(mData, root.getContext());
        mySelfListView.setAdapter(mAdapter);
        mySelfListView.setCallback(MqttUtils::getItems);
        mySelfListView.setOnItemClickListener(this::onItemClick);
        return root;
    }

    public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
        TrashSaveObj item = mData.get(position - 1);
        if (item.X <= 0 || item.Y <= 0) {
            Toast.makeText(view.getContext(), "无法定位垃圾桶", Toast.LENGTH_SHORT).show();
            return;
        }
        MainActivity.local(item);
    }
}