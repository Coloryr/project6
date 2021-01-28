#include "Arduino.h"
#include "MyBLE.h"
#include "Servo.h"
#include "main.h"
#include "NBIoT.h"
#include "MyIIC.h"
#include "VL53L0.h"

uint8_t mode;
bool Bluetooth_State;
bool NetWork_State;

void setup()
{
    Serial.begin(115200);
    //
    ThisServo = new Servo();
    IoT = new NBIoT();
    IIC = new MyIIC();
    VL53L0A = new VL53L0(VL53L0_A);
    VL53L0B = new VL53L0(VL53L0_B);

    if (VL53L0A->isok())
    {
        Serial.println("VL53L0 0 Start Done");
    }
    else
    {
        Serial.println("VL53L0 0 Start Fail");
    }

    if (VL53L0B->isok())
    {
        Serial.println("VL53L0 1 Start Done");
    }
    else
    {
        Serial.println("VL53L0 1 Start Fail");
    }

    // if (NetWork_State)
    // {
    //     BLE = new MyBLE(Server);
    // }
    // else
    // {
    //     BLE = new MyBLE(Client);
    // }
}

void loop()
{
    // BLE->Tick();
    // for (int d = 1; d < 180; d += 1)
    // {
    //     ThisServo->SetServo(d);
    //     Serial.printf("value=%d\n", d);
    //     delay(500);
    // }
}
