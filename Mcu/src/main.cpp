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

bool Close;
bool busy;
bool IsOpen;
bool SendOnce;
uint16_t timego = 0;
uint8_t State;
//0 ok
//1 初始化
//2 初始化完成
//3 距离传感器错误
//5 快满了
uint8_t Capacity;
uint8_t Time;

void longTask(void *arg)
{
    for (;;)
    {
        if (!IoT->isOnline())
        {
            busy = true;
            IoT->init();
            delay(100);
            IoT->setGnssOpen(true);
            busy = false;
        }
        else if (!IoT->isMqtt())
        {
            busy = true;
            IoT->startMqtt();
            busy = false;
        }
        else
        {
            busy = true;
            IoT->test();
            busy = false;
            if (SendOnce)
            {
                busy = true;
                IoT->sendSIM();
                delay(1000);
                IoT->readGnss();
                delay(1000);
                IoT->send();
                timego = 0;
                SendOnce = false;
                busy = false;
            }
            timego++;
            if (timego > 360)
            {
                timego = 0;
                busy = true;
                IoT->readGnss();
                delay(1000);
                IoT->send();
                busy = false;
            }
        }
        delay(5000);
    }
}

void tick()
{
    if (Time > 0)
    {
        Time--;
        if (Time == 0)
        {
            ThisServo->close();
        }
        return;
    }
    if (IO->readOpen())
    {
        IsOpen = true;
        Time = 40;
        ThisServo->open();
        return;
    }
    bool close = IO->isClose();
    bool close1 = IO->readClose();
    if (close && close1)
    {
        Close = true;
    }
    else
    {
        Close = false;
    }
    if (!close)
        return;
    if (VL53L0A->isOK())
    {
        VL53L0A->update();
    }
    if (VL53L0B->isOK())
    {
        VL53L0B->update();
    }
    double sum = 0;
    uint8_t count = 0;
    if (VL53L0A->status == 11)
    {
        if (VL53L0A->count[2] <= Distance)
        {
            double temp = (double)VL53L0A->count[2] / (double)Distance;
            sum += temp * 100;
            count++;
        }
    }
    if (VL53L0B->status == 11)
    {
        if (VL53L0B->count[2] <= Distance)
        {
            double temp = (double)VL53L0B->count[2] / (double)Distance;
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
        }
        else
        {
            State = 0;
        }
    }
}

void setup()
{
    State = 1;
    delay(200);
    Serial.begin(115200);
    Serial.setTimeout(100);
#ifdef DEBUG
    Serial.println("Start");
#endif
    ThisEEPROM = new EEPROM();
    ThisEEPROM->init();
    ThisServo = new Servo();
    IO = new IOInput();
    VL53L0A = new VL53L0(VL53L0_A, '0');
    VL53L0B = new VL53L0(VL53L0_B, '1');

    VL53L0A->check();
    VL53L0B->check();

    Up = new Upload();
    delay(2000);
    IoT = new NBIoT();

    xTaskCreate(longTask, "task", 8192, NULL, 3, NULL);
    // if (NetWork_State)
    // {
    //     BLE = new MyBLE(Server);
    // }
    // else
    // {
    //     BLE = new MyBLE(Client);
    // }
    State = 2;
}

void loop()
{
    tick();
    Up->tick();
    if (!busy)
        IoT->tick();
    delay(100);
}
