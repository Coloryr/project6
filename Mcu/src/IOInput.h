#ifndef IOINPUT_h
#define IOINPUT_h

#include "Arduino.h"
#include "main.h"

#define ADC_IN 34

#define Open_IN 12
#define Close_IN 14

class IOInput
{
private:
    uint16_t adcread();

public:
    IOInput();
    bool readopen();
    bool readclose();
    bool isclose();
};


#endif