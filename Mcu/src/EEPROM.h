#ifndef EEPROM_h
#define EEPROM_h

#include "Arduino.h"
#include "main.h"

#define EEPROM_Add 0xA0

#define I2C_EEPROM_SCL GPIO_NUM_26
#define I2C_EEPROM_SDA GPIO_NUM_25
#define I2C_EEPROM_NUM I2C_NUM_1

#define UUID_Add 0x00
#define SET_Add 0x10
#define IP_Add 0x20
#define Port_Add 0x24
#define Bit_Add 0x9e
#define Test_Add 0x67

#define test_data 0x21
#define bit_data 0x4d

class EEPROM
{
private:
    bool ok;
    void readall();
    void readuuid();
    void readip();
    void readset();
    
public:
    EEPROM();
    void init();
    void saveall();
    void saveuuid();
    void saveip();
    void saveset();
    bool isok();
};


#endif