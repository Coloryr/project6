#include "Arduino.h"
#include "main.h"
#include "MyIIC.h"
#include "EEPROM.h"

EEPROM *ThisEEPROM;

EEPROM::EEPROM()
{
}

void EEPROM::init()
{
    IIC->WriteBit(Test_Add, EEPROM_Add, test_data);
    delay(10);
    uint8_t data = IIC->ReadBit(Test_Add, EEPROM_Add);
    ok = data == test_data;
    if (ok)
    {
        Serial.println("EEPROM ok");
        data = IIC->ReadBit(Bit_Add, EEPROM_Add);
        if (data == bit_data)
        {
            Serial.println("EEPROM have data");
            readall();
        }
        else
        {
            Serial.println("EEPROM is null");
        }
        Serial.println("EEPROM init done");
    }
    else
    {
        Serial.println("EEPROM fail");
    }
}

void EEPROM::readall()
{
    readuuid();
    readip();
}
void EEPROM::readuuid()
{
}
void EEPROM::readip()
{
}
void EEPROM::saveall()
{
}
void EEPROM::saveuuid()
{
}
void EEPROM::saveip()
{
}
bool EEPROM::isok()
{
}