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
    EEPROMIIC->writeBit(Test_Add, EEPROM_Add, test_data);
    delay(10);
    uint8_t data = EEPROMIIC->readBit(Test_Add, EEPROM_Add);
    ok = data == test_data;
    if (ok)
    {
#ifdef DEBUG
        Serial.println("EEPROM ok");
#endif
        data = EEPROMIIC->readBit(Bit_Add, EEPROM_Add);
        if (data == bit_data)
        {
#ifdef DEBUG
            Serial.println("EEPROM have data");
#endif
            readAll();
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
            saveAll();
            EEPROMIIC->writeBit(Bit_Add, EEPROM_Add, bit_data);
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

void EEPROM::readAll()
{
    readUUID();
    readIP();
    readSet();
}
void EEPROM::readUUID()
{
    EEPROMIIC->read(UUID_Add, EEPROM_Add, UUID, 8);
    EEPROMIIC->read(UUID_Add + 8, EEPROM_Add, UUID + 8, 8);
#ifdef DEBUG
    Serial.print("UUID:");
    for (uint8_t a = 0; a < 16; a++)
    {
        Serial.printf("%c", UUID[a]);
    }
    Serial.println();
#endif
}
void EEPROM::readIP()
{
    uint8_t *buff = new uint8_t[6];
    EEPROMIIC->read(IP_Add, EEPROM_Add, buff, 6);
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
void EEPROM::readSet()
{
    uint8_t *buff = new uint8_t[8];
    EEPROMIIC->read(SET_Add, EEPROM_Add, buff, 8);
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

void EEPROM::saveAll()
{
    saveUUID();
    delay(10);
    saveIP();
    delay(10);
    saveSet();
    delay(10);
}
void EEPROM::saveUUID()
{
#ifdef DEBUG
    Serial.print("UUID:");
    for (uint8_t a = 0; a < 16; a++)
    {
        Serial.printf("%c", UUID[a]);
    }
    Serial.println();
#endif
    EEPROMIIC->write(UUID_Add, EEPROM_Add, UUID, 8);
    delay(10);
    EEPROMIIC->write(UUID_Add + 8, EEPROM_Add, UUID + 8, 8);
}
void EEPROM::saveIP()
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
    EEPROMIIC->write(IP_Add, EEPROM_Add, buff, 6);
    delete (buff);
}
void EEPROM::saveSet()
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
    EEPROMIIC->write(SET_Add, EEPROM_Add, buff, 8);
    delete (buff);
}
bool EEPROM::isOK()
{
    return ok;
}