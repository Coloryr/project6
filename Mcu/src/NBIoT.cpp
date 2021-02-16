#include "NBIoT.h"
#include "main.h"

NBIoT *IoT;

uint8_t UUID[16];
uint8_t User[16];
uint8_t Pass[16];

uint8_t IP[4];
uint16_t Port;

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
    // xTaskCreate(IoTRead, "IoT", 1024, NULL, 5, NULL);
    setGnssOpen(true);
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

void NBIoT::startSocket()
{
    if (!ok || !card || !online)
        return;
}
void NBIoT::startMqtt()
{
    if (!ok || !card || !online)
        return;
    Serial2.printf("AT+QMTOPEN=0,\"%s\",%d\n", IP, Port);
    delay(300);
    String data = Serial2.readString();
    data.trim();
#ifdef DEBUG
    Serial2.println(data);
#endif
    Serial2.printf("AT+QMTCONN=0,\"%s\",\"%s\",\"%s\"", UUID, User, Pass);
    data = Serial2.readString();
    data.trim();
#ifdef DEBUG
    Serial2.println(data);
#endif
}
void NBIoT::send(uint8_t *data)
{
    if (!ok || !card || !online)
        return;
    if (!socket && !mqtt)
        return;
}