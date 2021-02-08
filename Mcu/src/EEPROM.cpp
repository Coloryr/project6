#include "Arduino.h"
#include "main.h"
#include "MyIIC.h"
#include "EEPROM.h"
#include "NBIoT.h"
#include "tools.h"
#include "VL53L0.h"
#include "IOInput.h"
#include "tools.h"

EEPROM *ThisEEPROM;
MyIIC *EEPROMIIC = NULL;

EEPROM::EEPROM()
{
    if (EEPROMIIC == NULL)
    {
        EEPROMIIC = new MyIIC(I2C_EEPROM_SDA, I2C_EEPROM_SCL, I2C_EEPROM_NUM);
    }
    uint8_t *test = new uint8_t[64];
    Serial.print("test:");
    for (uint8_t a = 0; a < 16; a++)
    {
        Serial.printf("%d ", test[a]);
    }
    Serial.println();
    EEPROMIIC->Write(Test_Add, EEPROM_Add, test, 8);
    delay(100);
    Serial.println("write done");
    EEPROMIIC->Read(Test_Add, EEPROM_Add, test, 8);
    Serial.print("read:");
    for (uint8_t a = 0; a < 16; a++)
    {
        Serial.printf("%d ", test[a]);
    }
    Serial.println();
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
                Port[a] = 0;
            }
            saveall();
            EEPROMIIC->WriteBit(Bit_Add, EEPROM_Add, bit_data);
        }
#ifdef DEBUG
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
    EEPROMIIC->Read(UUID_Add, EEPROM_Add, UUID, 16);
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
    Port[0] = buff[4];
    Port[1] = buff[5];
    delete (buff);
}
void EEPROM::readset()
{
    uint8_t *buff = new uint8_t[6];
    EEPROMIIC->Read(SET_Add, EEPROM_Add, buff, 6);
    maxsize = makeuint16(buff[1], buff[0]);
    ADC_Low = makeuint16(buff[3], buff[2]);
    ADC_HIGH = makeuint16(buff[5], buff[4]);
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
    EEPROMIIC->Write(UUID_Add, EEPROM_Add, UUID, 16);
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
    EEPROMIIC->Write(IP_Add, EEPROM_Add, buff, 6);
    delete (buff);
}
void EEPROM::saveset()
{
    uint8_t *buff = new uint8_t[6];
    Mytow tow;
    tow.u16 = maxsize;
    buff[0] = tow.u8[0];
    buff[1] = tow.u8[1];
    tow.u16 = ADC_Low;
    buff[2] = tow.u8[0];
    buff[3] = tow.u8[1];
    tow.u16 = ADC_HIGH;
    buff[4] = tow.u8[0];
    buff[5] = tow.u8[1];
    EEPROMIIC->Write(SET_Add, EEPROM_Add, buff, 6);
    delete (buff);
}
bool EEPROM::isok()
{
    return ok;
}