#include "Arduino.h"
#include "Servo.h"
#include "main.h"
#include "NBIoT.h"
#include "MyIIC.h"
#include "VL53L0.h"
#include "EEPROM.h"
#include "IOInput.h"
#include "Upload.h"
#include "driver/uart.h"

RTC_DATA_ATTR bool Close;
RTC_DATA_ATTR bool busy;
RTC_DATA_ATTR bool IsOpen;
RTC_DATA_ATTR bool SendOnce;

RTC_DATA_ATTR bool Init = false;

RTC_DATA_ATTR uint16_t timego = 0;
RTC_DATA_ATTR uint16_t timego1 = 0;
RTC_DATA_ATTR uint8_t State;
//0 ok
//1 初始化
//2 初始化完成
//3 距离传感器错误
//5 快满了
//6 定位失效
uint8_t Capacity;

#define uS_TO_S_FACTOR 1000000 /* Conversion factor for micro seconds to seconds */
#define TIME_TO_SLEEP 5        /* Time ESP32 will go to sleep (in seconds) */

RTC_DATA_ATTR int bootCount = 0;

void get();

void longTask()
{
    if (!IoT.isOnline())
    {
        busy = true;
        IoT.init();
        delay(200);
        IoT.test();
        busy = false;
    }
    else if (!IoT.isMqtt())
    {
        busy = true;
        IoT.startMqtt();
        delay(200);
        IoT.sendSIM();
        busy = false;
    }

    if (SendOnce)
    {
#ifdef DEBUG
        Serial.println("上传数据");
#endif
        busy = true;
        Serial2.println("ATE0");
        IoT.setGnssOpen(true);
        for (;;)
        {
            if (IoT.readGnss())
            {
                State = 6;
                break;
            }
            State = 0;
            return;
        }
        get();
        delay(200);
        IoT.send();
        delay(200);
        IoT.sleep();
        timego = 0;
        timego1 = 0;
        SendOnce = false;
        busy = false;
    }

    timego++;
    timego1++;

    if (timego1 > 30)
    {
        timego1 = 0;
        busy = true;
        IoT.test();
        delay(200);
        IoT.sleep();
        busy = false;
    }
    if (timego > 360)
    {
        SendOnce = true;
    }
}

void tick()
{
    if (IsOpen)
    {
        ThisServo.close();
        IsOpen = false;
    }

    IO.init();
    Close = IO.isClose() && IO.readClose();
#ifdef DEBUG
    Serial.printf("关闭状态:%s\n", Close ? "true" : "false");
#endif
    if (!Close)
        return;
    get();
}

void get()
{
    VL53L0A.check();
    VL53L0B.check();
    if (VL53L0A.isOK())
    {
        VL53L0A.update();
    }
    if (VL53L0B.isOK())
    {
        VL53L0B.update();
    }
    double sum = 0;
    uint8_t count = 0;
    if (VL53L0A.status == 11)
    {
        if (VL53L0A.count[2] <= Distance)
        {
            double temp = (double)VL53L0A.count[2] / (double)Distance;
            sum += temp * 100;
            count++;
        }
    }
    if (VL53L0B.status == 11)
    {
        if (VL53L0B.count[2] <= Distance)
        {
            double temp = (double)VL53L0B.count[2] / (double)Distance;
            sum += temp * 100;
            count++;
        }
    }
    if (count == 0)
    {
        State = 3;
    }
    else
    {
        Capacity = sum / count;
        if (Capacity < 10)
        {
            State = 5;
            SendOnce = true;
        }
        else
        {
            if (State == 5)
            {
                SendOnce = true;
            }
            State = 0;
        }
    }
}

void Io_Read()
{
    if (IO.readOpen())
    {
        SendOnce = true;
        IsOpen = true;
        ThisServo.open();
    }
}

void print_wakeup_reason()
{
    esp_sleep_source_t wakeup_reason = esp_sleep_get_wakeup_cause();
    switch (wakeup_reason)
    {
    case ESP_SLEEP_WAKEUP_EXT0:
        Io_Read();
        break;
    case ESP_SLEEP_WAKEUP_TIMER:
        tick();
        longTask();
        if (!busy)
            IoT.tick();
        break;
    case ESP_SLEEP_WAKEUP_UNDEFINED:
        longTask();
        break;
    default:
        break;
    }
}

void setup()
{
    Serial.begin(115200);
    Serial.setTimeout(100);
    Serial2.begin(115200);
    Serial.println("start");
#ifdef SLEEP
    if (!Init)
    {
        State = 1;
        delay(200);
        IO.init();
        ThisEEPROM.init();
        ThisServo.close();
        VL53L0A.check();
        VL53L0B.check();
        Serial2.println("AT");
        delay(200);
        Serial2.println("ATE0");
        delay(200);
        IoT.init();
        State = 2;
        Init = true;
    }
#endif
#ifndef SLEEP
    IO.init();
    ThisEEPROM.init();
    ThisServo.close();
    VL53L0A.check();
    VL53L0B.check();
    Serial.println("init done");
#endif

#ifdef SLEEP
    print_wakeup_reason();
    esp_sleep_enable_ext0_wakeup(GPIO_NUM_12, 0);
    esp_sleep_enable_timer_wakeup(TIME_TO_SLEEP * uS_TO_S_FACTOR * 2);
    esp_deep_sleep_start();
#endif
}

#ifndef SLEEP
uint8_t count = 0;
#endif
void loop()
{
#ifndef SLEEP
    if (VL53L0A.isOK())
    {
        VL53L0A.update();
    }
    if (VL53L0B.isOK())
    {
        VL53L0B.update();
    }
    if (IO.readOpen())
    {
        ThisServo.open();
        count = 10;
    }
    else if (count == 0)
    {
        ThisServo.close();
        count = -1;
    }
    if (count > 0)
    {
        count--;
    }
    Up.tick();
    delay(200);
#endif
}
