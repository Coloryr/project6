#include "Arduino.h"
#include "MyBLE.h"
#include "Servo.h"
#include "main.h"
#include "NBIoT.h"

void setup()
{
    Serial.begin(115200);
    // BLE = new MyBLE(Client);
    ThisServo = new Servo();
    IoT = new NBIoT();
}

void loop()
{
    // BLE->TickClient();
    // for (int d = 1; d < 180; d += 1)
    // {
    //     ThisServo->SetServo(d);
    //     Serial.printf("value=%d\n", d);
    //     delay(500);
    // }
}
