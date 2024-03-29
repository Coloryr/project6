#include "NBIoT.h"
#include "main.h"
#include "IOInput.h"

NBIoT IoT;

RTC_DATA_ATTR uint8_t UUID[16];
RTC_DATA_ATTR uint8_t User[16];
RTC_DATA_ATTR uint8_t Pass[16];

RTC_DATA_ATTR uint8_t IP[4];
RTC_DATA_ATTR uint16_t Port;

const String TopicTrashServer = "trash/server";
const String TopicTrashClient = "trash/client";

RTC_DATA_ATTR String SelfTopic;
RTC_DATA_ATTR String SelfUUID;

uint8_t SIM[20];

RTC_DATA_ATTR String X;
RTC_DATA_ATTR String Y;
RTC_DATA_ATTR String Time_YMD;
RTC_DATA_ATTR String Time_HMS;

RTC_DATA_ATTR bool ok;
RTC_DATA_ATTR bool card;
RTC_DATA_ATTR bool socket;
RTC_DATA_ATTR bool online;
RTC_DATA_ATTR bool mqtt;

NBIoT::NBIoT()
{
}

void NBIoT::test()
{
    if (mqtt)
    {
#ifdef DEBUG
        Serial.println("连接测试");
#endif
        Serial2.readString();
        Serial2.println("AT+QMTCONN?");
        delay(500);
        String data = Serial2.readString();
        data.trim();
        if (!data.startsWith("+QMTCONN: 0,3"))
        {
#ifdef DEBUG
            Serial.println(data.c_str());
#endif
            mqtt = false;
        }
        else
        {
            mqtt = true;
        }

        return;
    }
}

void NBIoT::tick()
{
    if (Serial2.available() > 0)
    {
        String data = Serial2.readString();
        data.trim();
#ifdef DEBUG
        Serial.printf("收到数据:%s\n", data.c_str());
#endif
        if (data.startsWith("+QMTRECV: 0,"))
        {
            data.replace("+QMTRECV: 0,", "");
            data = data.substring(3);
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
                    timego = 400;
                }
            }
        }
        else if (data.startsWith("+QMTSTAT: 0,1"))
        {
            mqtt = false;
        }
    }
}

void NBIoT::init()
{
    ok = false;
    card = false;
    socket = false;
    online = false;
    mqtt = false;
    check();
    if (!ok)
        return;
    getCard();
    if (!card)
        return;
    if (getQuality() == 99)
        return;
    checkOnline();
    IoT.setGnssOpen(true);
}

void NBIoT::check()
{
    Serial2.readString();
    Serial2.println("ATE0");
    delay(300);
    String data = Serial2.readString();
    data.trim();
    if (data.startsWith("OK"))
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
        Serial.println(data.c_str());
#endif
        ok = false;
    }
}
void NBIoT::getCard()
{
    Serial2.readString();
    Serial2.println("AT+CIMI");
    delay(300);
    String data = Serial2.readString();
    data.trim();
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
}

void NBIoT::checkOnline()
{
    Serial2.readString();
    Serial2.println("AT+CGATT?");
    delay(300);
    String data = Serial2.readString();
    data.trim();
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
}

uint8_t NBIoT::getQuality()
{
    Serial2.readString();
    Serial2.println("AT+CESQ");
    delay(300);
    String data = Serial2.readString();
    data.trim();
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
    return 99;
}

void NBIoT::setGnssOpen(bool open)
{
#ifdef DEBUG
    Serial.printf("NB-IoT:GNSS模式设置为:%d\n", open);
#endif
    if (!open)
    {
        Serial2.println("AT+QGNSSC=0");
        return;
    }
    Serial2.readString();
    Serial2.println("AT+QGNSSC?");
    delay(500);
    String data = Serial2.readString();
    data.trim();
    if (!data.startsWith("+QGNSSC: 1"))
    {
        Serial2.println("AT+QGNSSC=1");
        delay(300);
        Serial2.println("AT+QGNSSAGPS=1");
        delay(200);
        Serial2.readString();
    }
    else
    {
        Serial.println("GPS已开启");
    }
}

#ifdef DEBUG
void gpsTest()
{
    Serial2.readString();
    Serial2.println("AT+QGNSSRD?");
    delay(200);
    String data = Serial2.readString();
    data.trim();
    Serial.println(data.c_str());
}
#endif

bool NBIoT::readGnss()
{
    Serial2.readString();
    Serial2.println("AT+QGNSSRD=\"NMEA/RMC\"");
    delay(200);
    String data = Serial2.readString();
    data.trim();
#ifdef DEBUG
    Serial.println(data.c_str());
#endif
    if (!data.startsWith("+QGNSSRD: $GNRMC,"))
    {
#ifdef DEBUG
        Serial.println("无效的定位");
        gpsTest();
#endif
        return false;
    }
    data.replace("+QGNSSRD: $GNRMC,", "");
    if (data[0] == ',')
    {
#ifdef DEBUG
        Serial.println("无效的定位");
        gpsTest();
#endif
        return false;
    }
    Time_HMS = data.substring(0, 9);
    data = data.substring(10);
    if (data[0] == 'V')
    {
        data = data.substring(8);
        Serial.println(data.c_str());
        Time_YMD = data.substring(0, 6);
        Serial.printf("当前时间:%s, %s\n", Time_YMD.c_str(), Time_HMS.c_str());
#ifdef DEBUG
        Serial.println("无效的定位");
        gpsTest();
#endif

        return false;
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
#ifdef DEBUG
    Serial.printf("当前时间:%s, %s\n", Time_YMD.c_str(), Time_HMS.c_str());
    Serial.printf("当前坐标:%s, %s\n", X.c_str(), Y.c_str());
#endif
    return true;
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

void NBIoT::startSocket()
{
    if (!ok || !card || !online)
        return;
}
void NBIoT::startMqtt()
{
    if (!ok || !card || !online)
        return;
#ifdef DEBUG
    Serial.println("正在断开MQTT服务器");
#endif
    Serial2.readString();
    Serial2.println("AT+QMTCLOSE=0");
    delay(300);
    Serial2.readString();
    Serial2.printf("AT+QMTOPEN=0,\"%d.%d.%d.%d\",%d", IP[0], IP[1], IP[2], IP[3], Port);
    Serial2.println();
    delay(2000);
    String data = Serial2.readString();
    data.trim();
    if (!data.endsWith("+QMTOPEN: 0,0") && !data.endsWith("+QMTSTAT: 0,3"))
    {
#ifdef DEBUG
        Serial.println("MQTT服务器连接失败");
        Serial.println(data.c_str());
#endif
        mqtt = false;
        return;
    }
#ifdef DEBUG
    Serial.println("MQTT服务器已连接");
#endif
    SelfUUID.clear();
    for (uint8_t a = 0; a < 16; a++)
    {
        SelfUUID += (char)UUID[a];
    }
    Serial2.readString();
    Serial2.printf("AT+QMTCONN=0,\"%s\",\"%s\",\"%s\"", SelfUUID.c_str(), User, Pass);
    Serial2.println();
    delay(2000);
    data = Serial2.readString();
    data.trim();
    if (!data.endsWith("+QMTCONN: 0,0,0"))
    {
#ifdef DEBUG
        Serial.println("MQTT服务器认证失败");
        Serial.println(data.c_str());
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
    Serial2.printf("AT+QMTSUB=0,2,\"%s\",2", SelfTopic.c_str());
    Serial2.println();
    Serial2.readString();
    mqtt = true;
    SendOnce = true;
}
void NBIoT::send()
{
    if (!ok || !card || !online)
        return;
    if (!socket && !mqtt)
        return;
    if (mqtt)
    {
#ifdef DEBUG
        Serial.printf("AT+QMTPUB=0,1,2,0,\"%s\",\"%s,%s,%s,%s,%s,%d,%d,%d,%d\"\n",
                      TopicTrashClient.c_str(),
                      SelfUUID.c_str(),
                      X.c_str(), Y.c_str(),
                      Time_YMD.c_str(), Time_HMS.c_str(),
                      Close, IO.readBattery(), State, Capacity);
#endif
        Serial2.printf("AT+QMTPUB=0,1,2,0,\"%s\",\"%s,%s,%s,%s,%s,%d,%d,%d,%d\"",
                       TopicTrashClient.c_str(),
                       SelfUUID.c_str(),
                       X.c_str(), Y.c_str(),
                       Time_YMD.c_str(), Time_HMS.c_str(),
                       Close, IO.readBattery(), State, Capacity);
        Serial2.println();
        delay(100);
        Serial2.readString();
    }
}
void NBIoT::sendSIM()
{
    if (!ok || !card || !online)
        return;
    if (!socket && !mqtt)
        return;
    if (mqtt)
    {
#ifdef DEBUG
        Serial.printf(" AT+QMTPUB=0,1,2,0,\"%s\",\"%s,%s\"\n",
                      TopicTrashClient.c_str(),
                      SelfUUID.c_str(), SIM);
#endif
        Serial2.printf("AT+QMTPUB=0,1,2,0,\"%s\",\"%s,%s\"",
                       TopicTrashClient.c_str(),
                       SelfUUID.c_str(), SIM);
        Serial2.println();
        delay(100);
        Serial2.readString();
    }
}

void NBIoT::sleep()
{
    if (!ok)
        return;
#ifdef DEBUG
    Serial.println("NB-IoT:进入低功耗");
#endif
    Serial2.println("AT+QSCLK=2");
    Serial2.println("AT+QNBIOTEVENT=1,1");
    delay(30);
    Serial2.readString();
}