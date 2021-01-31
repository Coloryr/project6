#include "Arduino.h"
#include "IOInput.h"
#include "main.h"

IOInput *IO;

uint16_t ADC_Low;
uint16_t ADC_HIGH;
bool last;

IOInput::IOInput()
{
    pinMode(ADC_IN, ANALOG);
    pinMode(Open_IN, INPUT_PULLUP);
    pinMode(Close_IN, INPUT_PULLUP);
}

uint16_t IOInput::adcread()
{
    return analogRead(ADC_IN);
}

bool IOInput::readopen()
{
    if (digitalRead(Open_IN) == LOW)
    {
        delay(10);
        if (digitalRead(Open_IN) == LOW)
        {
#ifdef DEBUG
            Serial.println("key open is down");
#endif
            return true;
        }
    }
    return false;
}
bool IOInput::readclose()
{
    if (digitalRead(Close_IN) == LOW)
    {
        delay(10);
        if (digitalRead(Close_IN) == LOW)
        {
#ifdef DEBUG
            Serial.println("key close is down");
#endif
            return true;
        }
    }
    return false;
}
bool IOInput::isclose()
{
    uint16_t temp = adcread();
#ifdef DEBUG
    Serial.printf("adc:%d\n", temp);
#endif
    if (last)
    {
        if (temp > ADC_HIGH)
        {
            last = false;
            return false;
        }
        else
        {
            return true;
        }
    }
    else
    {
        if (temp < ADC_Low)
        {
            last = true;
            return true;
        }
        else
        {
            return false;
        }
    }
}
