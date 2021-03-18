package com.coloryrtrash.app;

import android.app.Service;
import android.content.Intent;
import android.os.Binder;
import android.os.Handler;
import android.os.IBinder;
import android.os.Looper;
import android.util.Log;
import android.widget.Toast;
import androidx.annotation.Nullable;
import com.alibaba.fastjson.JSON;
import com.coloryrtrash.app.objs.DataArg;
import com.coloryrtrash.app.objs.DataPackObj;
import com.coloryrtrash.app.objs.DataType;
import com.coloryrtrash.app.objs.TrashSaveObj;
import com.coloryrtrash.app.ui.HomeFragment;
import com.coloryrtrash.app.ui.TrashFragment;
import org.eclipse.paho.android.service.MqttAndroidClient;
import org.eclipse.paho.client.mqttv3.*;

import java.nio.charset.StandardCharsets;
import java.util.List;

public class MqttUtils extends Service {

    private static final String BROKER_URL = "tcp://{0}:{1}";
    private String selfServerTopic;
    private String selfClientTopic;
    private MqttAndroidClient mqttClient;
    private String token;

    private class connectclass implements IMqttActionListener {
        @Override
        public void onSuccess(IMqttToken asyncActionToken) {
            selfServerTopic = DataArg.TopicAppServer + "/" + MainActivity.config.user;
            selfClientTopic = DataArg.TopicAppClient + "/" + MainActivity.config.user;
            int[] Qos = {2, 2};
            String[] topic1 = {selfServerTopic, DataArg.TopicAppServer};
            try {
                mqttClient.subscribe(topic1, Qos);
            } catch (Exception e) {
                e.printStackTrace();
            }
            MainActivity.isConnect();
        }

        @Override
        public void onFailure(IMqttToken asyncActionToken, Throwable exception) {
            Toast.makeText(MainActivity.MainActivity.getApplicationContext(), "服务器链接失败", Toast.LENGTH_LONG).show();
        }
    }
    private class reclass implements MqttCallback {
        @Override
        public void connectionLost(Throwable cause) {
            MainActivity.loginOut();
        }

        @Override
        public void messageArrived(String topic, MqttMessage temp) {
            if (topic.equalsIgnoreCase(selfServerTopic)) {
                String message = new String(temp.getPayload(), StandardCharsets.UTF_8);
                DataPackObj obj = JSON.parseObject(message, DataPackObj.class);
                Handler mainHandler = new Handler(Looper.getMainLooper());
                mainHandler.post(() -> {
                    if (obj.Type == DataType.Login) {
                        if (obj.Res) {
                            token = obj.Data;
                            MainActivity.config.token = token;
                            MainActivity.loginDone();
                            Toast.makeText(MainActivity.MainActivity.getApplicationContext(),
                                    "登录成功", Toast.LENGTH_SHORT).show();
                        } else {
                            MainActivity.isLogin = false;
                            Toast.makeText(MainActivity.MainActivity.getApplicationContext(),
                                    obj.Data, Toast.LENGTH_SHORT).show();
                        }
                        return;
                    } else if (obj.Type == DataType.CheckLogin) {
                        if (!obj.Res) {
                            Toast.makeText(MainActivity.MainActivity.getApplicationContext(),
                                    "自动登录失败", Toast.LENGTH_SHORT).show();
                            MainActivity.loginOut();
                        } else {
                            token = MainActivity.config.token;
                            MainActivity.loginDone();
                            Toast.makeText(MainActivity.MainActivity.getApplicationContext(),
                                    "已自动登录", Toast.LENGTH_SHORT).show();
                        }
                        return;
                    } else if (!obj.Res) {
                        Toast.makeText(MainActivity.MainActivity.getApplicationContext(),
                                obj.Data, Toast.LENGTH_SHORT).show();
                        MainActivity.loginOut();
                        return;
                    }
                    switch (obj.Type) {
                        case GetUserGroup:
                            if (obj.Data == null) {
                                Toast.makeText(MainActivity.MainActivity.getApplicationContext(),
                                        obj.Data1, Toast.LENGTH_SHORT).show();
                            } else {
                                HomeFragment.setGroup(obj.Data);
                                MainActivity.groupName = obj.Data;
                            }
                            break;
                        case GetUserTask:
                            if (obj.Data == null) {
                                Toast.makeText(MainActivity.MainActivity.getApplicationContext(),
                                        obj.Data1, Toast.LENGTH_SHORT).show();
                            } else {
                                List<TrashSaveObj> list = JSON.parseArray(obj.Data, TrashSaveObj.class);
                                TrashFragment.setList(list);
                                TrashFragment.setDone();
                            }
                            break;
                    }
                });
            } else if (topic.equalsIgnoreCase(DataArg.TopicAppServer)) {
                String message = new String(temp.getPayload(), StandardCharsets.UTF_8);
                DataPackObj obj = JSON.parseObject(message, DataPackObj.class);
                Handler mainHandler = new Handler(Looper.getMainLooper());
                mainHandler.post(() -> {
                    switch (obj.Type) {
                        case SetUserGroupBind:
                            if (MainActivity.groupName.equals(obj.Data)) {
                                getItems();
                            }
                            break;
                        case UpdataTrash:
                            if (MainActivity.groupName.equals(obj.Data)) {
                                List<TrashSaveObj> list = JSON.parseArray(obj.Data1, TrashSaveObj.class);
                                TrashFragment.setList(list);
                            }
                            break;
                        case Full:
                            MainActivity.full();
                            break;
                    }
                });
            }
        }

        @Override
        public void deliveryComplete(IMqttDeliveryToken token) {

        }
    }

    private final reclass re = new reclass();
    private final connectclass connect = new connectclass();

    public void start() {
        try {
            mqttClient = new MqttAndroidClient(MainActivity.MainActivity.getApplicationContext(), BROKER_URL
                    .replace("{0}", MainActivity.config.ip)
                    .replace("{1}", MainActivity.config.port + ""),
                    MainActivity.config.user);
            // MQTT的连接设置
            MqttConnectOptions options = new MqttConnectOptions();
            options.setCleanSession(false);
            options.setAutomaticReconnect(true);
            options.setUserName(MainActivity.config.user);
            options.setConnectionTimeout(10);
            options.setKeepAliveInterval(10);
            mqttClient.setCallback(re);
            mqttClient.connect(options, null, connect);
        } catch (Exception e) {
            Toast.makeText(MainActivity.MainActivity.getApplicationContext(), e.getMessage(), Toast.LENGTH_LONG).show();
            e.printStackTrace();
            MainActivity.loginOut();
        }
    }

    @Override
    public void onCreate() {
        Log.i("ColoryrTrash", "onCreate");
        super.onCreate();
    }


    @Override
    public void onDestroy() {
        Log.i("ColoryrTrash", "onDestroy");
        super.onDestroy();
    }

    private final IBinder binder = new LocalBinder();

    public class LocalBinder extends Binder {
        MqttUtils getService() {
            // Return this instance of LocalService so clients can call public methods
            return MqttUtils.this;
        }
    }

    @Nullable
    @Override
    public IBinder onBind(Intent intent) {
        return binder;
    }

    public void setToken(String token) {
        this.token = token;
    }

    final Handler mHandler = new Handler();

    private void toast(final CharSequence text) {
        mHandler.post(() -> Toast.makeText(MainActivity.MainActivity.getApplicationContext(), text, Toast.LENGTH_SHORT).show());
    }

    public void stop() {
        try {
            if (mqttClient != null)
                mqttClient.disconnect(0);
        } catch (Exception e) {
            toast(e.getMessage());
            e.printStackTrace();
        }
    }

    public void getItems() {
        DataPackObj obj = new DataPackObj();
        obj.Token = token;
        obj.Type = DataType.GetUserTask;
        obj.Data = MainActivity.config.user;
        send(obj);
    }

    public void getInfo() {
        DataPackObj obj = new DataPackObj();
        obj.Token = token;
        obj.Type = DataType.GetUserGroup;
        obj.Data = MainActivity.config.user;
        send(obj);
    }

    public void checkLogin() {
        Handler mainHandler = new Handler(Looper.getMainLooper());
        mainHandler.post(() -> {
            Toast.makeText(MainActivity.MainActivity.getApplicationContext(),
                    "自动登录中", Toast.LENGTH_SHORT).show();
        });
        DataPackObj obj = new DataPackObj();
        obj.Type = DataType.CheckLogin;
        obj.Token = token;
        send(obj);
    }

    public void login(String pass) {
        DataPackObj obj = new DataPackObj();
        obj.Type = DataType.Login;
        obj.Data = Tools.genSHA1(pass);
        send(obj);
    }

    private void send(Object object) {
        if (mqttClient.isConnected()) {
            String message = JSON.toJSONString(object);
            try {
                mqttClient.publish(selfClientTopic,
                        message.getBytes(StandardCharsets.UTF_8),
                        2,
                        false);
            } catch (Exception e) {
                e.printStackTrace();
            }
        }
    }
}