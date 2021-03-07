#ifndef IOINPUT_h
#define IOINPUT_h

#include "stm32f1xx_hal.h"

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

};

#endif