#include "Arduino.h"
#include "MyBLE.h"

void setup()
{
    Serial.begin(115200);
    BLE = new MyBLE(Client);
}

void loop()
{
    BLE->TickClient();
}

