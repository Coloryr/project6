#include "Arduino.h"
#include "main.h"
#include "MyIIC.h"
#include "EEPROM.h"
#include "NBIoT.h"
#include "tools.h"
#include "VL53L0.h"
#include "IOInput.h"
#include "tools.h"
#include "Servo.h"

EEPROM *ThisEEPROM;
MyIIC *EEPROMIIC = NULL;

EEPROM::EEPROM()
{
    if (EEPROMIIC == NULL)
    {
        EEPROMIIC = new MyIIC(I2C_EEPROM_SDA, I2C_EEPROM_SCL, I2C_EEPROM_NUM);
    }
}

void EEPROM::init()
{
    EEPROMIIC->WriteBit(Test_Add, EEPROM_Add, test_data);
    delay(10);
    uint8_t data = EEPROMIIC->ReadBit(Test_Add, EEPROM_Add);
    ok = data == test_data;
    if (ok)
    {
#ifdef DEBUG
        Serial.println("EEPROM ok");
#endif
        data = EEPROMIIC->ReadBit(Bit_Add, EEPROM_Add);
        if (data == bit_data)
        {
#ifdef DEBUG
            Serial.println("EEPROM have data");
#endif
            readall();
        }
        else
        {
#ifdef DEBUG
            Serial.println("EEPROM is null");
#endif
            uint8_t a = 0;
            for (a = 0; a < 16; a++)
            {
                UUID[a] = '0';
            }
            for (a = 0; a < 4; a++)
            {
                IP[a] = 0;
            }
            for (a = 0; a < 2; a++)
            {
                Port = 0;
            }
            saveall();
            EEPROMIIC->WriteBit(Bit_Add, EEPROM_Add, bit_data);
        }
#ifdef DEBUG
        Serial.printf("IP:%d.%d.%d.%d, Port:%d\n", IP[0], IP[1], IP[2], IP[3], Port);
        Serial.printf("ADC_High:%d,ADC_Low:%d,Distance:%d\n", ADC_HIGH, ADC_Low, Distance);
        Serial.printf("ServoOpen:%d,ServoClose:%d\n", openset, closeset);
        Serial.println("EEPROM init done");
        ok = true;
#endif
    }
    else
    {
#ifdef DEBUG
        Serial.println("EEPROM fail");
#endif
        ok = false;
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
    EEPROMIIC->Read(UUID_Add, EEPROM_Add, UUID, 8);
    EEPROMIIC->Read(UUID_Add + 8, EEPROM_Add, UUID + 8, 8);
#ifdef DEBUG
    Serial.print("UUID:");
    for (uint8_t a = 0; a < 16; a++)
    {
        Serial.printf("%c", UUID[a]);
    }
    Serial.println();
#endif
}
void EEPROM::readip()
{
    uint8_t *buff = new uint8_t[6];
    EEPROMIIC->Read(IP_Add, EEPROM_Add, buff, 6);
    IP[0] = buff[0];
    IP[1] = buff[1];
    IP[2] = buff[2];
    IP[3] = buff[3];
    Mytow tow;
    tow.u8[1] = buff[4];
    tow.u8[0] = buff[5];
    Port = tow.u16;
    delete (buff);
}
void EEPROM::readset()
{
    uint8_t *buff = new uint8_t[8];
    EEPROMIIC->Read(SET_Add, EEPROM_Add, buff, 8);
    Mytow tow;
    tow.u8[0] = buff[0];
    tow.u8[1] = buff[1];
    Distance = tow.u16;
    tow.u8[1] = buff[2];
    tow.u8[0] = buff[3];
    ADC_Low = tow.u16;
    tow.u8[1] = buff[4];
    tow.u8[0] = buff[5];
    ADC_HIGH = tow.u16;
    openset = buff[6];
    closeset = buff[7];
    delete (buff);
}

void EEPROM::saveall()
{
    saveuuid();
    delay(10);
    saveip();
    delay(10);
    saveset();
    delay(10);
}
void EEPROM::saveuuid()
{
#ifdef DEBUG
    Serial.print("UUID:");
    for (uint8_t a = 0; a < 16; a++)
    {
        Serial.printf("%c", UUID[a]);
    }
    Serial.println();
#endif
    EEPROMIIC->Write(UUID_Add, EEPROM_Add, UUID, 8);
    delay(10);
    EEPROMIIC->Write(UUID_Add + 8, EEPROM_Add, UUID + 8, 8);
}
void EEPROM::saveip()
{
    uint8_t *buff = new uint8_t[6];
    buff[0] = IP[0];
    buff[1] = IP[1];
    buff[2] = IP[2];
    buff[3] = IP[3];
    Mytow tow;
    tow.u16 = Port;
    buff[4] = tow.u8[0];
    buff[5] = tow.u8[1];
    EEPROMIIC->Write(IP_Add, EEPROM_Add, buff, 6);
    delete (buff);
}
void EEPROM::saveset()
{
    uint8_t *buff = new uint8_t[8];
    Mytow tow;
    tow.u16 = Distance;
    buff[0] = tow.u8[0];
    buff[1] = tow.u8[1];
    tow.u16 = ADC_Low;
    buff[2] = tow.u8[0];
    buff[3] = tow.u8[1];
    tow.u16 = ADC_HIGH;
    buff[4] = tow.u8[0];
    buff[5] = tow.u8[1];
    buff[6] = openset;
    buff[7] = closeset;
    EEPROMIIC->Write(SET_Add, EEPROM_Add, buff, 8);
    delete (buff);
}
bool EEPROM::isok()
{
    return ok;
}