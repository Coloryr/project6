package com.coloryrtrash.app;

import android.annotation.SuppressLint;
import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.Button;
import android.widget.TextView;
import com.coloryrtrash.app.objs.TrashSaveObj;

import java.util.List;

public class ItemAdapter extends BaseAdapter {
    private final List<TrashSaveObj> mData;
    private final Context mContext;

    public ItemAdapter(List<TrashSaveObj> mData, Context mContext) {
        this.mData = mData;
        this.mContext = mContext;
    }

    @Override
    public int getCount() {
        return mData.size();
    }

    @Override
    public Object getItem(int position) {
        return null;
    }

    @Override
    public long getItemId(int position) {
        return position;
    }

    @SuppressLint("ViewHolder")
    @Override
    public View getView(int position, View convertView, ViewGroup parent) {
        if (convertView == null)
            convertView = LayoutInflater.from(mContext).inflate(R.layout.list_item, parent, false);
        TrashSaveObj item = mData.get(position);
        TextView textView = convertView.findViewById(R.id.state);
        textView.setText(item.State.toString());
        textView = convertView.findViewById(R.id.time_);
        textView.setText(R.string.item_time);
        textView = convertView.findViewById(R.id.time);
        textView.setText(item.Time);
        textView = convertView.findViewById(R.id.uuid_);
        textView.setText(R.string.item_uuid);
        textView = convertView.findViewById(R.id.uuid);
        textView.setText(item.UUID);
        textView = convertView.findViewById(R.id.nick_);
        textView.setText(R.string.item_nick);
        textView = convertView.findViewById(R.id.nick);
        textView.setText(item.Nick);
        textView = convertView.findViewById(R.id.capacity_);
        textView.setText(R.string.item_capacity);
        textView = convertView.findViewById(R.id.capacity);
        textView.setText(item.Capacity + "");
        return convertView;
    }
}
