#include "Arduino.h"
#include "MyBLE.h"
#include "Servo.h"
#include "main.h"
#include "NBIoT.h"

uint8_t mode;
bool Bluetooth_State;
bool NetWork_State;

void setup()
{
    Serial.begin(115200);
    //
    ThisServo = new Servo();
    IoT = new NBIoT();

    if (NetWork_State)
    {
        BLE = new MyBLE(Server);
    }
    else
    {
        BLE = new MyBLE(Client);
    }
}

void loop()
{
    BLE->Tick();
    // for (int d = 1; d < 180; d += 1)
    // {
    //     ThisServo->SetServo(d);
    //     Serial.printf("value=%d\n", d);
    //     delay(500);
    // }
}
