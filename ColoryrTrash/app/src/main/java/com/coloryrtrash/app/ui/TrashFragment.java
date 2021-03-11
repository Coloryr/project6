package com.coloryrtrash.app.ui;

import android.annotation.SuppressLint;
import android.os.*;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import androidx.annotation.NonNull;
import androidx.fragment.app.Fragment;
import com.coloryrtrash.app.ItemAdapter;
import com.coloryrtrash.app.PullRefresh;
import com.coloryrtrash.app.R;
import com.coloryrtrash.app.objs.ItemState;
import com.coloryrtrash.app.objs.TrashSaveObj;

import java.util.LinkedList;
import java.util.List;

public class TrashFragment extends Fragment {

    @SuppressLint("StaticFieldLeak")
    private static PullRefresh mySelfListView;

    @SuppressLint("StaticFieldLeak")
    private static View root;

    private final List<TrashSaveObj> mData = new LinkedList<>();
    private ItemAdapter mAdapter = null;

    public View onCreateView(@NonNull LayoutInflater inflater,
                             ViewGroup container, Bundle savedInstanceState) {
        if (root == null)
            root = inflater.inflate(R.layout.fragment_list, container, false);
        mySelfListView = root.findViewById(R.id.listview);
        initData(root);
        return root;
    }

    public void initData(View root) {

        for (int i = 0; i < 2; i++) {
            TrashSaveObj obj = new TrashSaveObj();
            obj.Nick = "新的垃圾桶";
            obj.State = ItemState.正常;
            obj.UUID = "123456789" + i;
            obj.Capacity = i;
            mData.add(obj);
        }

        mAdapter = new ItemAdapter(mData, root.getContext());
        mySelfListView.setAdapter(mAdapter);
        mySelfListView.setCallback(() -> {
            new Thread(() -> {
                try {
                    Thread.sleep(2000);
                } catch (InterruptedException e) {
                    e.printStackTrace();
                }
                TrashSaveObj obj = new TrashSaveObj();
                obj.Nick = "新的垃圾桶";
                obj.State = ItemState.正常;
                obj.UUID = "123456789";
                mData.add(obj);
                Handler mainHandler = new Handler(Looper.getMainLooper());
                mainHandler.post(() -> {
                    mAdapter.notifyDataSetChanged();
                    mySelfListView.updateHeaderResult();
                });
            }).start();
        });
    }
}