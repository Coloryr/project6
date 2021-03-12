#include "main.h"
#include "EEPROM.h"
#include "NBIoT.h"
#include "VL53L0.h"
#include "IOInput.h"
#include "Servo.h"

EEPROM ThisEEPROM;

void EEPROM::init()
{
    uint8_t data = test_data;
    HAL_I2C_Mem_Write(&hi2c1, EEPROM_Add, Test_Add, I2C_MEMADD_SIZE_8BIT, &data, 1, 1000);
    HAL_Delay(10);
    HAL_I2C_Mem_Read(&hi2c1, EEPROM_Add, Test_Add, I2C_MEMADD_SIZE_8BIT, &data, 1, 1000);
    ok = data == test_data;
    if (ok)
    {
#ifdef DEBUG
        Serial.println("EEPROM ok");
#endif
        HAL_I2C_Mem_Read(&hi2c1, EEPROM_Add, Bit_Add, I2C_MEMADD_SIZE_8BIT, &data, 1, 1000);
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
            data = bit_data;
            HAL_I2C_Mem_Write(&hi2c1, EEPROM_Add, Bit_Add, I2C_MEMADD_SIZE_8BIT, &data, 1, 1000);
        }
#ifdef DEBUG
        Serial.printf("IP:%d.%d.%d.%d, Port:%d\n", IP[0], IP[1], IP[2], IP[3], Port);
        Serial.printf("ADC_High:%d,ADC_Low:%d,Distance:%d\n", ADC_HIGH, ADC_Low, Distance);
        Serial.printf("ServoOpen:%d,ServoClose:%d\n", openset, closeset);
        Serial.print("UUID:");
        for (uint8_t a = 0; a < 16; a++)
        {
            Serial.printf("%c", UUID[a]);
        }
        Serial.println();
        Serial.print("User:");
        for (uint8_t a = 0; a < 16; a++)
        {
            Serial.printf("%c", User[a]);
        }
        Serial.println();
        Serial.print("Pass:");
        for (uint8_t a = 0; a < 16; a++)
        {
            Serial.printf("%c", Pass[a]);
        }
        Serial.println();
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
    readMqtt();
}
void EEPROM::readUUID()
{
    HAL_I2C_Mem_Read(&hi2c1, EEPROM_Add, UUID_Add, I2C_MEMADD_SIZE_8BIT, UUID, 8, 1000);
    HAL_I2C_Mem_Read(&hi2c1, EEPROM_Add + 8, UUID_Add, I2C_MEMADD_SIZE_8BIT, UUID + 8, 8, 1000);
}
void EEPROM::readIP()
{
    uint8_t *buff = new uint8_t[6];
    HAL_I2C_Mem_Read(&hi2c1, EEPROM_Add, IP_Add, I2C_MEMADD_SIZE_8BIT, buff, 6, 1000);
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
    HAL_I2C_Mem_Read(&hi2c1, EEPROM_Add, SET_Add, I2C_MEMADD_SIZE_8BIT, buff, 8, 1000);
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

void EEPROM::readMqtt()
{
    HAL_I2C_Mem_Read(&hi2c1, EEPROM_Add, User_Add, I2C_MEMADD_SIZE_8BIT, User, 8, 1000);
    HAL_I2C_Mem_Read(&hi2c1, EEPROM_Add, User_Add + 8, I2C_MEMADD_SIZE_8BIT, User + 8, 8, 1000);
    HAL_I2C_Mem_Read(&hi2c1, EEPROM_Add, Pass_Add, I2C_MEMADD_SIZE_8BIT, Pass, 8, 1000);
    HAL_I2C_Mem_Read(&hi2c1, EEPROM_Add, Pass_Add + 8, I2C_MEMADD_SIZE_8BIT, Pass + 8, 8, 1000);
}

void EEPROM::saveAll()
{
    saveUUID();
    HAL_Delay(10);
    saveIP();
    HAL_Delay(10);
    saveSet();
    HAL_Delay(10);
    saveMqtt();
    HAL_Delay(10);
}
void EEPROM::saveUUID()
{
    HAL_I2C_Mem_Write(&hi2c1, EEPROM_Add, UUID_Add, I2C_MEMADD_SIZE_8BIT, UUID, 8, 1000);
    HAL_Delay(10);
    HAL_I2C_Mem_Write(&hi2c1, EEPROM_Add, UUID_Add + 8, I2C_MEMADD_SIZE_8BIT, UUID + 8, 8, 1000);
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
    HAL_I2C_Mem_Write(&hi2c1, EEPROM_Add, IP_Add, I2C_MEMADD_SIZE_8BIT, UUID, 6, 1000);
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
    HAL_I2C_Mem_Write(&hi2c1, EEPROM_Add, SET_Add, I2C_MEMADD_SIZE_8BIT, buff, 8, 1000);
    delete (buff);
}
void EEPROM::saveMqtt()
{
    HAL_I2C_Mem_Write(&hi2c1, EEPROM_Add, User_Add, I2C_MEMADD_SIZE_8BIT, User, 8, 1000);
    HAL_Delay(10);
    HAL_I2C_Mem_Write(&hi2c1, EEPROM_Add, User_Add + 8, I2C_MEMADD_SIZE_8BIT, User + 8, 8, 1000);
    HAL_Delay(10);
    HAL_I2C_Mem_Write(&hi2c1, EEPROM_Add, Pass_Add, I2C_MEMADD_SIZE_8BIT, Pass, 8, 1000);
    HAL_Delay(10);
    HAL_I2C_Mem_Write(&hi2c1, EEPROM_Add, Pass_Add + 8, I2C_MEMADD_SIZE_8BIT, Pass + 8, 8, 1000);
}

bool EEPROM::isOK()
{
    return ok;
}