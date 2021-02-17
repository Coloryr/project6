#include "NBIoT.h"
#include "main.h"
#include "IOInput.h"

NBIoT *IoT;

uint8_t UUID[16];
uint8_t User[16];
uint8_t Pass[16];

uint8_t IP[4];
uint16_t Port;

const String TopicTrashServer = "trash/server";
const String TopicTrashClient = "trash/client";

String SelfTopic;

uint8_t SIM[20];

String X;
String Y;
String Time_YMD;
String Time_HMS;

bool busy;

void IoTRead(void *arg)
{
#ifdef DEBUG
    Serial.println("开始读数据");
#endif
    for (;;)
    {
        if (!busy && Serial2.available() > 0)
        {
            Serial.println("收到数据");
            String data = Serial2.readString();
            data.trim();
#ifdef DEBUG
            Serial.println(data.c_str());
#endif
            if (data.startsWith("+QMTRECV: 0,"))
            {
                data.replace("+QMTRECV: 0,", "");
                data = data.substring(3);
#ifdef DEBUG
                Serial.println(data.c_str());
#endif
                int index = data.indexOf(',');
                String topic = data.substring(0, index - 1);
                String data1 = data.substring(index + 2, data.length() - 1);
#ifdef DEBUG
                Serial.printf("Topic:%s, Data:%s\n", topic.c_str(), data1.c_str());
#endif
                if (topic.equals(SelfTopic))
                {
                    if (data1.equals("Up"))
                    {
                        SendOnce = true;
                    }
                }
            }
            else if (data.startsWith("+QMTSTAT: 0,1"))
            {
                IoT->mqttDown();
            }
        }
        delay(10);
    }
}

NBIoT::NBIoT()
{
    Serial2.begin(115200);
    Serial2.println("AT");
    delay(500);
    Serial2.println("ATE0");
    delay(500);
    init();
    startRead();
}

void NBIoT::init()
{
    ok = false;
    card = false;
    socket = false;
    online = false;
    mqtt = false;
    Serial2.flush();
    check();
    if (!ok)
        return;
    getCard();
    if (!card)
        return;
    if (getQuality() == 99)
        return;
    checkOnline();
    setGnssOpen(true);
}

void NBIoT::startRead()
{
    xTaskCreate(IoTRead, "IoT", 2048, NULL, 5, NULL);
}

void NBIoT::check()
{
    busy = true;
    Serial2.println("ATE0");
    delay(300);
    String data = Serial2.readString();
    data.trim();
#ifdef DEBUG
    Serial.println(data.c_str());
#endif
    if (data.equalsIgnoreCase("OK"))
    {
#ifdef DEBUG
        Serial.println("NB-IoT:初始化成功");
#endif
        ok = true;
    }
    else
    {
#ifdef DEBUG
        Serial.println("NB-IoT:初始化失败");
#endif
        ok = false;
    }
    busy = false;
}
void NBIoT::getCard()
{
    busy = true;
    Serial2.flush();
    Serial2.println("AT+CIMI");
    delay(300);
    String data = Serial2.readString();
    data.trim();
#ifdef DEBUG
    Serial.println(data.c_str());
#endif
    if (data.equalsIgnoreCase("ERROR"))
    {
#ifdef DEBUG
        Serial.println("NB-IoT:SIM卡错误");
#endif
        card = false;
    }
    else if (data.endsWith("OK"))
    {
#ifdef DEBUG
        Serial.println("NB-IoT:SIM卡正常");
#endif
        for (uint8_t a = 0; a < data.length(); a++)
        {
            SIM[a] = data[a];
            if (data[a] == 13 || data[a] == 10)
            {
#ifdef DEBUG
                Serial.print("SIM号:");
                Serial.write(SIM, a);
                Serial.println();
#endif
                break;
            }
        }
        card = true;
    }
    busy = false;
}

void NBIoT::checkOnline()
{
    busy = true;
    Serial2.flush();
    Serial2.println("AT+CGATT?");
    delay(300);
    String data = Serial2.readString();
    data.trim();
#ifdef DEBUG
    Serial.println(data.c_str());
#endif
    if (data.equalsIgnoreCase("ERROR"))
    {
#ifdef DEBUG
        Serial.println("NB-IoT:入网失败");
#endif
    }
    else
    {
        if (data.startsWith("+CGATT: 1"))
        {
#ifdef DEBUG
            Serial.println("NB-IoT:入网成功");
#endif
            online = true;
        }
        else
        {
            Serial.println("NB-IoT:入网失败");
            online = false;
        }
    }
    busy = false;
}

uint8_t NBIoT::getQuality()
{
    busy = true;
    Serial2.flush();
    Serial2.println("AT+CESQ");
    delay(300);
    String data = Serial2.readString();
    data.trim();
#ifdef DEBUG
    Serial.println(data.c_str());
#endif
    if (data.equalsIgnoreCase("ERROR"))
    {
#ifdef DEBUG
        Serial.println("NB-IoT:没有信号");
#endif
        return 99;
    }
    else
    {
        if (data.startsWith("+CESQ:"))
        {
            data.remove(0, 7);
            int local = data.indexOf(',');
            String data1 = data.substring(0, local);
            uint8_t quality = data1.toInt();
#ifdef DEBUG
            Serial.printf("NB-IoT:信号强度:%d\n", quality);
#endif
            return quality;
        }
    }
    busy = false;
    return 99;
}

bool NBIoT::setGnssOpen(bool open)
{
    busy = true;
    Serial2.flush();
    Serial2.println("AT+QGNSSC?");
    delay(500);
    String data = Serial2.readString();
    data.trim();
#ifdef DEBUG
    Serial.println(data.c_str());
#endif
    bool state;
    state = data.startsWith("+QGNSSC: 1");
    if (open != state)
    {
        if (open && !state)
            Serial2.println("AT+QGNSSC=1");
        else if (!open && state)
            Serial2.println("AT+QGNSSC=0");
        data = Serial2.readString();
        data.trim();
#ifdef DEBUG
        Serial.println(data.c_str());
#endif
    }
    if (data.endsWith("OK"))
    {
#ifdef DEBUG
        Serial.printf("NB-IoT:GNSS模式设置为:%d\n", open);
#endif
        Serial2.println("AT+QGNSSAGPS=1");
        delay(300);
        data = Serial2.readString();
        data.trim();
#ifdef DEBUG
        Serial.println(data.c_str());
#endif
        busy = false;
        return true;
    }
    busy = false;
    return false;
}

void NBIoT::readGnss()
{
    busy = true;
    Serial2.flush();
    Serial2.println("AT+QGNSSRD=\"NMEA/RMC\"");
    delay(200);
    String data = Serial2.readString();
    data.trim();
#ifdef DEBUG
    Serial.println(data.c_str());
#endif
    data.replace("+QGNSSRD: $GNRMC,", "");
    if (data[0] == ',')
    {
        busy = false;
        return;
    }
    Time_HMS = data.substring(0, 9);
    data = data.substring(10);
    if (data[0] == 'V')
    {
#ifdef DEBUG
        Serial.println("无效的定位");
#endif
        data = data.substring(8);
        if (data[0] == ',')
        {
#ifdef DEBUG
            Serial.println("无效的时间");
#endif
            busy = false;
            return;
        }
        else
        {
            Time_YMD = data.substring(0, 6);
        }
        busy = false;
        return;
    }
    else
    {
#ifdef DEBUG
        Serial.println("有效的定位");
#endif
        data = data.substring(2);
        X = data.substring(0, 9);
        data = data.substring(12);
        Y = data.substring(0, 10);
        data = data.substring(20);
        Time_YMD = data.substring(0, 6);
    }
    delay(200);
#ifdef DEBUG
    Serial.printf("当前时间:%s, %s\n", Time_YMD.c_str(), Time_HMS.c_str());
    Serial.printf("当前坐标:%s, %s\n", X.c_str(), Y.c_str());
#endif
    busy = false;
}

bool NBIoT::isOK()
{
    return ok;
}

bool NBIoT::haveCard()
{
    return card;
}

bool NBIoT::isOnline()
{
    return online;
}

bool NBIoT::isMqtt()
{
    return mqtt;
}
bool NBIoT::isSocket()
{
    return socket;
}

void NBIoT::mqttDown()
{
    mqtt = false;
}

void NBIoT::startSocket()
{
    if (!ok || !card || !online)
        return;
}
void NBIoT::startMqtt()
{
    if (!ok || !card || !online)
        return;
    busy = true;
    Serial2.flush();
#ifdef DEBUG
    Serial.println("正在断开MQTT服务器");
#endif
    Serial2.println("AT+QMTCLOSE=0");
    delay(300);
    Serial2.flush();
    Serial2.printf("AT+QMTOPEN=0,\"%d.%d.%d.%d\",%d", IP[0], IP[1], IP[2], IP[3], Port);
    Serial2.println();
    delay(300);
    Serial2.flush();
    delay(10000);
    String data = Serial2.readString();
    data.trim();
#ifdef DEBUG
    Serial.println(data.c_str());
#endif
    if (!data.startsWith("+QMTOPEN: 0,0"))
    {
#ifdef DEBUG
        mqtt = false;
        return;
        Serial.println("MQTT服务器连接失败");
#endif
    }
#ifdef DEBUG
    Serial.println("MQTT服务器已连接");
#endif
    Serial2.printf("AT+QMTCONN=0,\"%s\",\"%s\",\"%s\"", UUID, User, Pass);
    Serial2.println();
    delay(300);
    Serial2.flush();
    delay(2000);
    data = Serial2.readString();
    data.trim();
#ifdef DEBUG
    Serial.println(data.c_str());
#endif
    if (!data.startsWith("+QMTCONN: 0,0,0"))
    {
#ifdef DEBUG
        Serial.println("MQTT服务器认证失败");
#endif
        mqtt = false;
        return;
    }
#ifdef DEBUG
    Serial.println("MQTT服务器认证成功");
#endif
    SelfTopic = TopicTrashServer + "/";
    for (uint8_t a = 0; a < 16; a++)
    {
        SelfTopic += (char)UUID[a];
    }
    Serial2.printf("AT+QMTSUB=0,1,\"%s\",1", TopicTrashServer.c_str());
    Serial2.println();
    delay(2000);
    data = Serial2.readString();
    data.trim();
#ifdef DEBUG
    Serial.println(data.c_str());
#endif
    Serial2.printf("AT+QMTSUB=0,2,\"%s\",1", SelfTopic.c_str());
    Serial2.println();
    delay(2000);
    data = Serial2.readString();
    data.trim();
#ifdef DEBUG
    Serial.println(data.c_str());
#endif
    mqtt = true;
    SendOnce = true;
    busy = false;
}
void NBIoT::send()
{
    if (!ok || !card || !online)
        return;
    if (!socket && !mqtt)
        return;
    if (mqtt)
    {
        busy = true;
        Serial2.flush();
#ifdef DEBUG
        Serial.printf("AT+QMTPUB=0,1,1,0,\"%s\",\"%s,%s,%s,%s,%s,%d,%d,%d,%d\"\n",
                      TopicTrashClient.c_str(),
                      UUID,
                      X.c_str(), Y.c_str(),
                      Time_YMD.c_str(), Time_HMS.c_str(),
                      Close, IO->readBattery(), State, Capacity);
#endif
        Serial2.flush();
        Serial2.printf("AT+QMTPUB=0,1,1,0,\"%s\",\"%s,%s,%s,%s,%s,%d,%d,%d,%d\"",
                       TopicTrashClient.c_str(),
                       UUID,
                       X.c_str(), Y.c_str(),
                       Time_YMD.c_str(), Time_HMS.c_str(),
                       Close, IO->readBattery(), State, Capacity);
        Serial2.println();
        delay(500);
        String data = Serial2.readString();
        data.trim();
#ifdef DEBUG
        Serial.println(data.c_str());
#endif
        if (!data.startsWith("OK"))
        {
            mqtt = false;
        }
    }
    busy = false;
}
void NBIoT::sendSIM()
{
    if (!ok || !card || !online)
        return;
    if (!socket && !mqtt)
        return;
    if (mqtt)
    {
        busy = true;
        Serial2.flush();
#ifdef DEBUG
        Serial.printf("AT+QMTPUB=0,1,1,0,\"%s\",\"%s,%s\"\n",
                      TopicTrashClient.c_str(),
                      UUID, SIM);
#endif
        Serial2.printf("AT+QMTPUB=0,1,1,0,\"%s\",\"%s,%s\"",
                       TopicTrashClient.c_str(),
                       UUID, SIM);
        Serial2.println();
        delay(500);
        String data = Serial2.readString();
        data.trim();
#ifdef DEBUG
        Serial.println(data.c_str());
#endif
        if (!data.startsWith("OK"))
        {
            mqtt = false;
        }
    }
    busy = false;
}