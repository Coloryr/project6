#ifndef EEPROM_h
#define EEPROM_h

#include "stm32f1xx_hal.h"

#define EEPROM_Add 0xA0

#define UUID_Add 0x00
#define SET_Add 0x20
#define IP_Add 0x30
#define User_Add 0x40
#define Pass_Add 0x50
#define Bit_Add 0x9e
#define Test_Add 0x67

#define test_data 0x22
#define bit_data 0x4c

class EEPROM
{
private:
    bool ok;
    void readAll();
    void readUUID();
    void readIP();
    void readSet();
    void readMqtt();
    
public:
    EEPROM();
    void init();
    void saveAll();
    void saveUUID();
    void saveIP();
    void saveSet();
    void saveMqtt();
    bool isOK();
};


#endif