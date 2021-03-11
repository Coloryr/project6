package com.coloryrtrash.app.ui;

import android.annotation.SuppressLint;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.Switch;
import android.widget.TextView;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.fragment.app.Fragment;
import com.coloryrtrash.app.MainActivity;
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

        login.setOnClickListener(this::onClick);

        ip.setText(MainActivity.config.ip);
        port.setText(MainActivity.config.port + "");
        user.setText(MainActivity.config.user);
        auto.setChecked(MainActivity.config.auto);

        return root;
    }

    private void onClick(View v) {
        if (!MainActivity.isLogin) {
            if (ip.getText().toString().isEmpty()) {
                Toast.makeText(v.getContext(), "IP为空", Toast.LENGTH_SHORT).show();
                return;
            }
            if (port.getText().toString().isEmpty()) {
                Toast.makeText(v.getContext(), "Port为空", Toast.LENGTH_SHORT).show();
                return;
            }
            if (user.getText().toString().isEmpty()) {
                Toast.makeText(v.getContext(), "User为空", Toast.LENGTH_SHORT).show();
                return;
            }
            if (pass.getText().toString().isEmpty()) {
                Toast.makeText(v.getContext(), "密码为空", Toast.LENGTH_SHORT).show();
                return;
            }
            MainActivity.config.ip = ip.getText().toString();
            MainActivity.config.port = Integer.parseInt(port.getText().toString());
            MainActivity.config.user = user.getText().toString();
            MainActivity.config.auto = auto.isChecked();
            MainActivity.config.token = "";
            MainActivity.start(pass.getText().toString());
            MainActivity.save();
        } else {
            MainActivity.loginOut();
            Toast.makeText(v.getContext(), "已登出", Toast.LENGTH_SHORT).show();
        }
    }

    public void close() {
        ip.setEnabled(false);
        port.setEnabled(false);
        user.setEnabled(false);
        pass.setEnabled(false);
        auto.setEnabled(false);
        login.setText(R.string.login_out_button);
    }

    public void open() {
        ip.setEnabled(true);
        port.setEnabled(true);
        user.setEnabled(true);
        pass.setEnabled(true);
        auto.setEnabled(true);
        login.setText(R.string.login_button);
    }
}