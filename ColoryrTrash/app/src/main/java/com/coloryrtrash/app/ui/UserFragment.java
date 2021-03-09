package com.coloryrtrash.app.ui;

import android.annotation.SuppressLint;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.Switch;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import com.coloryrtrash.app.R;

public class UserFragment extends Fragment {

    @SuppressLint("StaticFieldLeak")
    private static TextView ip;
    @SuppressLint("StaticFieldLeak")
    private static TextView port;
    @SuppressLint("StaticFieldLeak")
    private static TextView user;
    @SuppressLint("StaticFieldLeak")
    private static TextView pass;
    @SuppressLint({"UseSwitchCompatOrMaterialCode", "StaticFieldLeak"})
    private static Switch auto;
    @SuppressLint("StaticFieldLeak")
    private static Button login;

    @SuppressLint("StaticFieldLeak")
    private static View root;

    public View onCreateView(@NonNull LayoutInflater inflater,
                             ViewGroup container, Bundle savedInstanceState) {
        if (root == null)
            root = inflater.inflate(R.layout.fragment_user, container, false);
        ip = root.findViewById(R.id.textIP);
        port = root.findViewById(R.id.textPort);
        user = root.findViewById(R.id.textUser);
        pass = root.findViewById(R.id.textPass);

        auto = root.findViewById(R.id.auto_switch);
        auto.setText(R.string.auto_login);

        login = root.findViewById(R.id.login_button);

        TextView view = root.findViewById(R.id.ip_);
        view.setText(R.string.ip);
        view = root.findViewById(R.id.port_);
        view.setText(R.string.port);
        view = root.findViewById(R.id.user_);
        view.setText(R.string.user);
        view = root.findViewById(R.id.pass_);
        view.setText(R.string.password);

        return root;
    }
}