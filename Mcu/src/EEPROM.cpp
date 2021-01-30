#include "Arduino.h"
#include "main.h"
#include "MyIIC.h"
#include "EEPROM.h"
#include "NBIoT.h"
#include "tools.h"
#include "VL53L0.h"

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
            uint8_t a = 0;
            for (a = 0; a < 16; a++)
            {
                UUID[a] = 0;
            }
            for (a = 0; a < 4; a++)
            {
                IP[a] = 0;
            }
            for (a = 0; a < 2; a++)
            {
                Port[a] = 0;
            }
            saveall();
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
    readset();
}
void EEPROM::readuuid()
{
    IIC->Read(UUID_Add, EEPROM_Add, UUID, 16);
}
void EEPROM::readip()
{
    uint8_t *buff = new uint8_t[6];
    IIC->Read(UUID_Add, EEPROM_Add, buff, 6);
    IP[0] = buff[0];
    IP[1] = buff[1];
    IP[2] = buff[2];
    IP[3] = buff[3];
    Port[0] = buff[4];
    Port[1] = buff[5];
    delete (buff);
}
void EEPROM::readset()
{
    Mytow tow;
    IIC->Read(SET_Add, EEPROM_Add, tow.u8, 2);
    maxsize = tow.u16;
}

void EEPROM::saveall()
{
    saveuuid();
    saveip();
    saveset();
}
void EEPROM::saveuuid()
{
    IIC->Write(UUID_Add, EEPROM_Add, UUID, 16);
}
void EEPROM::saveip()
{
    uint8_t *buff = new uint8_t[6];
    buff[0] = IP[0];
    buff[1] = IP[1];
    buff[2] = IP[2];
    buff[3] = IP[3];
    buff[4] = Port[0];
    buff[5] = Port[1];
    IIC->Write(UUID_Add, EEPROM_Add, buff, 6);
    delete (buff);
}
void EEPROM::saveset()
{
    Mytow tow;
    tow.u16 = maxsize;
    IIC->Write(SET_Add, EEPROM_Add, tow.u8, 2);
}
bool EEPROM::isok()
{
    return ok;
}