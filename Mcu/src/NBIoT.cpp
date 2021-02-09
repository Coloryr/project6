#include "NBIoT.h"
#include "main.h"

NBIoT *IoT;

uint8_t UUID[16];
uint8_t IP[4];
uint16_t Port;

void IoTRead(void *arg)
{
    for (;;)
    {
        if (Serial2.available() > 0)
        {
            String data = Serial2.readString();
        }
        delay(10);
    }
}

NBIoT::NBIoT()
{
    Serial2.begin(115200);
    Serial2.println("ATE0");
    delay(20);
    Serial2.flush();
    Serial2.println("AT");
    delay(20);
    String data = Serial2.readString();
    data.replace("\n", "");
    Serial.println(data.c_str());
    if (data.equalsIgnoreCase("OK"))
    {
        Serial.println("NB-IoT OK");
        xTaskCreate(IoTRead, "IoT", 1024, NULL, 5, NULL);
    }
    else
    {
        NetWork_State = false;
    }
}
