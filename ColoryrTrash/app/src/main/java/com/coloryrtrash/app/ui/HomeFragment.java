package com.coloryrtrash.app.ui;

import android.annotation.SuppressLint;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.fragment.app.FragmentManager;
import com.coloryrtrash.app.R;

public class HomeFragment extends Fragment {
    @SuppressLint("StaticFieldLeak")
    private static TextView user;
    @SuppressLint("StaticFieldLeak")
    private static TextView group;

    @SuppressLint("StaticFieldLeak")
    private static View root;

    public View onCreateView(@NonNull LayoutInflater inflater,
                             ViewGroup container, Bundle savedInstanceState) {
        if (root == null)
            root = inflater.inflate(R.layout.fragment_home, container, false);
        user = root.findViewById(R.id.textView6);
        user.setText(R.string.user_no_login);
        group = root.findViewById(R.id.textGroup);
        TextView temp = root.findViewById(R.id.textView7);
        temp.setText(R.string.group);
        return root;
    }

    public static void setUser(String text) {
        user.setText(text);
    }

    public static void setGroup(String text) {
        group.setText(text);
    }

    public static void clear() {
        user.setText(R.string.user_no_login);
        group.setText("");
    }
}