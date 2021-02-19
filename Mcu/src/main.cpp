#include "Arduino.h"
#include "MyBLE.h"
#include "Servo.h"
#include "main.h"
#include "NBIoT.h"
#include "MyIIC.h"
#include "VL53L0.h"
#include "EEPROM.h"
#include "IOInput.h"
#include "Upload.h"

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
uint8_t Capacity;

#define uS_TO_S_FACTOR 1000000 /* Conversion factor for micro seconds to seconds */
#define TIME_TO_SLEEP 5        /* Time ESP32 will go to sleep (in seconds) */

RTC_DATA_ATTR int bootCount = 0;

void longTask()
{
    timego++;
    timego1++;

    if (!IoT.isOnline())
    {
        busy = true;
        IoT.init();
        delay(200);
        IoT.test();
        delay(200);
        IoT.sleep();
        busy = false;
    }
    else if (!IoT.isMqtt())
    {
        busy = true;
        IoT.startMqtt();
        delay(200);
        IoT.sendSIM();
        delay(200);
        IoT.sleep();
        busy = false;
    }

    if (SendOnce)
    {
#ifdef DEBUG
        Serial.println("上传数据");
#endif
        busy = true;
        IoT.setGnssOpen(true);
        delay(200);
        IoT.readGnss();
        delay(200);
        IoT.setGnssOpen(false);
        delay(200);
        IoT.send();
        delay(200);
        IoT.sleep();
        timego = 0;
        timego1 = 0;
        SendOnce = false;
        busy = false;
    }

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

    Close = IO.isClose() && IO.readClose();
    if (!Close)
        return;
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
    SendOnce = true;
    if (IO.readOpen())
    {
        IsOpen = true;
        ThisServo.open();
    }
}

void print_wakeup_reason()
{
    esp_sleep_source_t wakeup_reason = esp_sleep_get_wakeup_cause();
#ifdef DEBUG
    Serial.println("低功耗唤醒");
    switch (wakeup_reason)
    {
    case ESP_SLEEP_WAKEUP_UNDEFINED:
        Serial.println("In case of deep sleep, reset was not caused by exit from deep sleep");
        break;
    case ESP_SLEEP_WAKEUP_ALL:
        Serial.println("Not a wakeup cause, used to disable all wakeup sources with esp_sleep_disable_wakeup_source");
        break;
    case ESP_SLEEP_WAKEUP_EXT0:
        Serial.println("Wakeup caused by external signal using RTC_IO");
        break;
    case ESP_SLEEP_WAKEUP_EXT1:
        Serial.println("Wakeup caused by external signal using RTC_CNTL");
        break;
    case ESP_SLEEP_WAKEUP_TIMER:
        Serial.println("Wakeup caused by timer");
        break;
    case ESP_SLEEP_WAKEUP_TOUCHPAD:
        Serial.println("Wakeup caused by touchpad");
        break;
    case ESP_SLEEP_WAKEUP_ULP:
        Serial.println("Wakeup caused by ULP program");
        break;
    case ESP_SLEEP_WAKEUP_GPIO:
        Serial.println("Wakeup caused by GPIO (light sleep only)");
        break;
    case ESP_SLEEP_WAKEUP_UART:
        Serial.println("Wakeup caused by UART (light sleep only)");
        break;
    default:
        Serial.println("Wakeup was not caused by deep sleep");
        break;
    }
#endif
    switch (wakeup_reason)
    {
    case ESP_SLEEP_WAKEUP_EXT0:
        Io_Read();
        break;
    case ESP_SLEEP_WAKEUP_TIMER:
        tick();
        longTask();
        Up.tick();
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
    Serial2.setTimeout(100);
#ifdef DEBUG
    Serial.println("Start");
#endif
    if (!Init)
    {
        State = 1;
        delay(200);
        ThisEEPROM.init();
        VL53L0A.check();
        VL53L0B.check();
        Serial2.println("AT");
        delay(200);
        Serial2.println("ATE0");
        delay(200);
        IoT.init();
        delay(2000);
        State = 2;
        Init = true;
    }

    print_wakeup_reason();
    esp_sleep_enable_ext0_wakeup(GPIO_NUM_12, 0);
    esp_sleep_enable_ext0_wakeup(GPIO_NUM_16, 0);
    esp_sleep_enable_timer_wakeup(TIME_TO_SLEEP * uS_TO_S_FACTOR * 2);
    esp_deep_sleep_start();
}

void loop()
{
}
