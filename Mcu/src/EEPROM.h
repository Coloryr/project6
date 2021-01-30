#ifndef EEPROM_h
#define EEPROM_h

#include "Arduino.h"
#include "main.h"

#define EEPROM_Add 0xA0

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
    
public:
    EEPROM();
    void init();
    void saveall();
    void saveuuid();
    void saveip();
    bool isok();
};


#endif