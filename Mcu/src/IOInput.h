#ifndef IOINPUT_h
#define IOINPUT_h

#include "Arduino.h"
#include "main.h"

#define ADC_IN 34

#define Open_IN 12
#define Close_IN 14

extern uint16_t ADC_Low;
extern uint16_t ADC_HIGH;

class IOInput
{
private:
public:
    IOInput();
    uint16_t adcread();
    bool readopen();
    bool readclose();
    bool isclose();
};


#endif