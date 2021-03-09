#ifndef IOINPUT_h
#define IOINPUT_h

#include "main.h"

#define ADC_IN 34
#define Battery_IN 35

#define Open_IN 12
#define Close_IN 14

extern uint16_t ADC_Low;
extern uint16_t ADC_HIGH;

class IOInput
{
private:
public:
    IOInput();
    uint16_t readADC();
    bool readOpen();
    bool readClose();
    bool isClose();
    uint16_t readBattery();
    void init();

};

#endif