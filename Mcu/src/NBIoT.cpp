#include "NBIoT.h"
#include "main.h"

NBIoT *IoT;

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
    if(data.equalsIgnoreCase("OK"))
    {
        Serial.println("NB-IOT OK");
    }
}
