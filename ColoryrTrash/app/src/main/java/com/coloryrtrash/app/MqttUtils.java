package com.coloryrtrash.app;

import android.annotation.SuppressLint;
import android.app.Notification;
import android.app.Service;
import android.content.Intent;
import android.os.Binder;
import android.os.Handler;
import android.os.IBinder;
import android.os.Looper;
import android.widget.Toast;
import com.alibaba.fastjson.JSON;
import com.coloryrtrash.app.objs.DataArg;
import com.coloryrtrash.app.objs.DataPackObj;
import com.coloryrtrash.app.objs.DataType;
import com.coloryrtrash.app.objs.TrashSaveObj;
import com.coloryrtrash.app.ui.HomeFragment;
import com.coloryrtrash.app.ui.TrashFragment;
import org.eclipse.paho.android.service.MqttAndroidClient;
import org.eclipse.paho.client.mqttv3.*;
import org.eclipse.paho.client.mqttv3.persist.MemoryPersistence;

import java.nio.charset.StandardCharsets;
import java.util.List;

public class MqttUtils extends Service {

    public static final String BROKER_URL = "tcp://{0}:{1}";
    private static String selfServerTopic;
    private static String selfClientTopic;
    @SuppressLint("StaticFieldLeak")
    private static MqttAndroidClient mqttClient;
    public static String token;

    private static class Re implements MqttCallback {
        @Override
        public void connectionLost(Throwable cause) {
            MainActivity.isRun = false;
            System.out.println("连接失败---");
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

    @Override
    public void onCreate() {
        super.onCreate();
        startForeground(2, new Notification());
    }

    @Override
    public IBinder onBind(Intent intent) {
        return null;
    }

    public static void start() {
        try {
            mqttClient = new MqttAndroidClient(MainActivity.MainActivity.getApplicationContext(), BROKER_URL
                    .replace("{0}", MainActivity.config.ip)
                    .replace("{1}", MainActivity.config.port + ""),
                    MainActivity.config.user, new MemoryPersistence());
            // MQTT的连接设置
            MqttConnectOptions options = new MqttConnectOptions();
            options.setCleanSession(false);
            options.setAutomaticReconnect(true);
            options.setUserName(MainActivity.config.user);
            options.setConnectionTimeout(2);
            options.setKeepAliveInterval(2);
            mqttClient.setCallback(new Re());
            mqttClient.connect(options, null, new IMqttActionListener() {
                @Override
                public void onSuccess(IMqttToken asyncActionToken) {
                    selfServerTopic = DataArg.TopicAppServer + "/" + MainActivity.config.user;
                    selfClientTopic = DataArg.TopicAppClient + "/" + MainActivity.config.user;
                    int[] Qos = {2, 2};
                    String[] topic1 = {selfServerTopic, DataArg.TopicAppServer};
                    try {
                        mqttClient.subscribe(topic1, Qos);
                    } catch (MqttException e) {
                        e.printStackTrace();
                    }
                    MainActivity.isRun = true;
                    MainActivity.isConnect();
                }

                @Override
                public void onFailure(IMqttToken asyncActionToken, Throwable exception) {

                }
            });
        } catch (MqttException e) {
            Toast.makeText(MainActivity.MainActivity.getApplicationContext(), e.getMessage(), Toast.LENGTH_LONG).show();
            e.printStackTrace();
            MainActivity.isRun = false;
        }
    }

    @Override
    public void onDestroy() {
        stopSelf();
        stop();
        super.onDestroy();
    }

    public static void stop() {
        try {
            mqttClient.disconnect(0);
            MainActivity.isRun = false;
        } catch (MqttException e) {
            Toast.makeText(MainActivity.MainActivity.getApplicationContext(), e.getMessage(), Toast.LENGTH_LONG).show();
            e.printStackTrace();
        }
    }

    public static void getItems() {
        DataPackObj obj = new DataPackObj();
        obj.Token = token;
        obj.Type = DataType.GetUserTask;
        obj.Data = MainActivity.config.user;
        send(obj);
    }

    public static void getInfo() {
        DataPackObj obj = new DataPackObj();
        obj.Token = token;
        obj.Type = DataType.GetUserGroup;
        obj.Data = MainActivity.config.user;
        send(obj);
    }

    public static void checkLogin() {
        Handler mainHandler = new Handler(Looper.getMainLooper());
        mainHandler.post(() -> {
            Toast.makeText(MainActivity.MainActivity.getApplicationContext(),
                    "自动登录中", Toast.LENGTH_SHORT).show();
        });
        DataPackObj obj = new DataPackObj();
        obj.Type = DataType.CheckLogin;
        obj.Token = token;
        obj.Data = Tools.genSHA1(MainActivity.pass);
        send(obj);
    }

    public static void Login() {
        DataPackObj obj = new DataPackObj();
        obj.Type = DataType.Login;
        obj.Data = Tools.genSHA1(MainActivity.pass);
        MainActivity.pass = "";
        send(obj);
    }

    private static void send(Object object) {
        if (mqttClient.isConnected()) {
            String message = JSON.toJSONString(object);
            try {
                mqttClient.publish(selfClientTopic,
                        message.getBytes(StandardCharsets.UTF_8),
                        2,
                        false);
            } catch (MqttException e) {
                e.printStackTrace();
            }
        }
    }
}