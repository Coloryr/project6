#include "NBIoT.h"
#include "main.h"

NBIoT *IoT;

uint8_t UUID[16];

uint8_t IP[4];
uint16_t Port;

uint8_t SIM[20];

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
    readGnss();
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
        Serial2.println("AT+QGNSSAGPS=1");
        delay(300);
        data = Serial2.readString();
        data.trim();
#ifdef DEBUG
        Serial.println(data.c_str());
#endif
        return true;
    }
    return false;
}

void NBIoT::readGnss()
{
    // Serial2.println("AT+QGNSSRD=\"NMEA/RMC\"");
    Serial2.println("AT+QGNSSRD?");
    delay(200);
    String data = Serial2.readString();
    data.trim();
#ifdef DEBUG
    Serial.println(data.c_str());
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
void NBIoT::startMqtt(uint8_t *User, uint8_t *Pass)
{
    if (!ok || !card || !online)
        return;
}
void NBIoT::send(uint8_t *data)
{
    if (!ok || !card || !online)
        return;
    if (!socket && !mqtt)
        return;
}