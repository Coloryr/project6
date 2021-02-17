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

void IoTRead(void *arg)
{
    for (;;)
    {
        if (Serial2.available() > 0)
        {
            String data = Serial2.readString();
            Serial.print(data.c_str());
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
    xTaskCreate(IoTRead, "IoT", 1024, NULL, 5, NULL);
}

void NBIoT::check()
{
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
}
void NBIoT::getCard()
{
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
}

void NBIoT::checkOnline()
{
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
}

uint8_t NBIoT::getQuality()
{
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
    return 99;
}

bool NBIoT::setGnssOpen(bool open)
{
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
//         Serial2.println("AT+QGNSSAGPS=1");
//         delay(300);
//         data = Serial2.readString();
//         data.trim();
#ifdef DEBUG
        Serial.println(data.c_str());
#endif
        return true;
    }
    return false;
}

void NBIoT::readGnss()
{
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
#ifdef DEBUG
        Serial.println("无效的数据");
#endif
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
            return;
        }
        else
        {
            Time_YMD = data.substring(0, 6);
            return;
        }
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
#ifdef DEBUG
    Serial.printf("当前时间:%s, %s\n", Time_YMD.c_str(), Time_HMS.c_str());
    Serial.printf("当前坐标:%s, %s\n", X.c_str(), Y.c_str());
#endif
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
    Serial2.println("AT+QMTCLOSE=0");
    delay(300);
    Serial2.flush();
#ifdef DEBUG
    Serial.printf("AT+QMTOPEN=0,\"%d.%d.%d.%d\",%d\n", IP[0], IP[1], IP[2], IP[3], Port);
#endif
    Serial2.printf("AT+QMTOPEN=0,\"%d.%d.%d.%d\",%d", IP[0], IP[1], IP[2], IP[3], Port);
    Serial2.println();
    delay(300);
    Serial2.flush();
    delay(5000);
    String data = Serial2.readString();
    data.trim();
#ifdef DEBUG
    Serial.println(data.c_str());
#endif
    if (data.startsWith("+QMTOPEN: 0,0"))
    {
#ifdef DEBUG
        Serial.println("MQTT服务器已连接");
#endif
#ifdef DEBUG
        Serial.printf("AT+QMTCONN=0,\"%s\",\"%s\",\"%s\"\n", UUID, User, Pass);
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
            mqtt = false;
            return;
        }
        SelfTopic = TopicTrashClient + "/";
        for (uint8_t a = 0; a < 16; a++)
        {
            SelfTopic += (char)UUID[a];
        }
#ifdef DEBUG
        Serial.printf("AT+QMTSUB=0,1,\"%s\",1\n", TopicTrashServer.c_str());
#endif
        Serial2.printf("AT+QMTSUB=0,1,\"%s\",1", TopicTrashServer.c_str());
        Serial2.println();
        delay(2000);
        data = Serial2.readString();
        data.trim();
#ifdef DEBUG
        Serial.println(data.c_str());
#endif
#ifdef DEBUG
        Serial.printf("AT+QMTSUB=0,1,\"%s\",1\n", SelfTopic.c_str());
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
    }
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
        Serial.printf("AT+QMTPUB=0,1,1,0,\"%s\",\"%s,%s,%s,%s,%s,%s,%d,%d\"\n", SelfTopic.c_str(),
                      UUID, X.c_str(), Y.c_str(), Time_YMD.c_str(), Time_HMS.c_str(), Close, IO->readBattery());
#endif
    }
}