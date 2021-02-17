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
bool IsOpen;
uint8_t Capacity;
uint8_t Time;

void tick()
{
    if (Time > 0)
    {
        Time--;
        if (Time == 0)
        {
            ThisServo->close();
        }
    }
    if (IO->readOpen())
    {
        IsOpen = true;
        Time = 80;
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
            double temp = VL53L0A->count[2] / Distance;
            sum = temp * 100;
            count++;
        }
    }
    if (VL53L0B->status == 11)
    {
        if (VL53L0B->count[2] <= Distance)
        {
            double temp = VL53L0B->count[2] / Distance;
            sum = temp * 100;
            count++;
        }
    }
    Capacity = sum / count;
}

void setup()
{
    delay(2000);
    Serial.begin(115200);
    Serial.setTimeout(100);
#ifdef DEBUG
    Serial.println("Start");
#endif
    ThisServo = new Servo();
    IoT = new NBIoT();
    ThisEEPROM = new EEPROM();
    IO = new IOInput();
    VL53L0A = new VL53L0(VL53L0_A, '0');
    VL53L0B = new VL53L0(VL53L0_B, '1');

    VL53L0A->check();
    VL53L0B->check();

    ThisEEPROM->init();

    Up = new Upload();

    // if (NetWork_State)
    // {
    //     BLE = new MyBLE(Server);
    // }
    // else
    // {
    //     BLE = new MyBLE(Client);
    // }
    IoT->startMqtt();
    IoT->startRead();
}

void loop()
{
    tick();
    delay(50);
}
