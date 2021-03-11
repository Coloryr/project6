package com.coloryrtrash.app;

import android.app.Service;
import android.content.Intent;
import android.os.Handler;
import android.os.IBinder;
import android.os.Looper;
import android.widget.Toast;
import com.alibaba.fastjson.JSON;
import com.coloryrtrash.app.objs.DataArg;
import com.coloryrtrash.app.objs.DataPackObj;
import com.coloryrtrash.app.objs.DataType;
import org.eclipse.paho.client.mqttv3.*;
import org.eclipse.paho.client.mqttv3.persist.MemoryPersistence;

import java.nio.charset.StandardCharsets;

public class MqttUtils extends Service implements MqttCallback {

    public static final String BROKER_URL = "tcp://{0}:{1}";
    private static String selfServerTopic;
    private static String selfClientTopic;
    private static IMqttClient mqttClient;
    private static MqttConnectOptions options;
    public static String token;

    public IBinder onBind(Intent intent) {
        return null;
    }

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        try {
            mqttClient = new MqttClient(BROKER_URL
                    .replace("{0}", MainActivity.config.ip)
                    .replace("{1}", MainActivity.config.port + ""),
                    MainActivity.config.user, new MemoryPersistence());
            // MQTT的连接设置
            options = new MqttConnectOptions();
            options.setCleanSession(false);
            options.setUserName(MainActivity.config.user);
            options.setConnectionTimeout(10);
            options.setKeepAliveInterval(20);
            mqttClient.setCallback(this);
            mqttClient.connect(options);
            selfServerTopic = DataArg.TopicAppServer + "/" + MainActivity.config.user;
            selfClientTopic = DataArg.TopicAppClient + "/" + MainActivity.config.user;
            int[] Qos = {2, 2};
            String[] topic1 = {selfServerTopic, DataArg.TopicAppServer};
            mqttClient.subscribe(topic1, Qos);
            MainActivity.isRun = true;
            MainActivity.isConnect();
        } catch (MqttException e) {
            Toast.makeText(getApplicationContext(), e.getMessage(), Toast.LENGTH_LONG).show();
            e.printStackTrace();
            MainActivity.isRun = false;
        }
        return super.onStartCommand(intent, flags, startId);
    }

    @Override
    public void onDestroy() {
        try {
            mqttClient.disconnect(0);
            MainActivity.isRun = false;
        } catch (MqttException e) {
            Toast.makeText(getApplicationContext(), "Something went wrong!" + e.getMessage(), Toast.LENGTH_LONG).show();
            e.printStackTrace();
        }
    }

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
            if (obj.Type == DataType.Login) {
                if (obj.Res) {
                    token = obj.Data;
                    MainActivity.config.token = token;
                    Handler mainHandler = new Handler(Looper.getMainLooper());
                    mainHandler.post(() -> {
                        MainActivity.loginDone();
                        Toast.makeText(MainActivity.MainActivity.getApplicationContext(),
                                "登录成功", Toast.LENGTH_SHORT).show();
                    });

                } else {
                    MainActivity.isLogin = false;
                    Handler mainHandler = new Handler(Looper.getMainLooper());
                    mainHandler.post(() -> {
                        Toast.makeText(MainActivity.MainActivity.getApplicationContext(),
                                obj.Data, Toast.LENGTH_SHORT).show();
                    });
                }
            } else if (obj.Type == DataType.CheckLogin) {
                Handler mainHandler = new Handler(Looper.getMainLooper());
                mainHandler.post(() -> {
                    if (!obj.Res) {
                        Toast.makeText(MainActivity.MainActivity.getApplicationContext(),
                                "自动登录失败", Toast.LENGTH_SHORT).show();
                    } else {
                        token = MainActivity.config.token;
                        MainActivity.loginDone();
                        Toast.makeText(MainActivity.MainActivity.getApplicationContext(),
                                "已自动登录", Toast.LENGTH_SHORT).show();
                    }
                    if (!obj.Res) {
                        Toast.makeText(MainActivity.MainActivity.getApplicationContext(),
                                obj.Data, Toast.LENGTH_SHORT).show();
                        MainActivity.loginOut();
                    }
                });
            }
        } else if (topic.equalsIgnoreCase(DataArg.TopicAppServer)) {

        }
    }

    @Override
    public void deliveryComplete(IMqttDeliveryToken token) {
        System.out.println("--deliveryComplete--成功发布某一消息后的回调方法");
    }

    public static void checkLogin()
    {
        DataPackObj obj = new DataPackObj();
        obj.Type = DataType.Login;
        obj.Token = token;
        obj.Data = Tools.genSHA1(MainActivity.pass);
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