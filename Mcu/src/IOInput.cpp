#include "Arduino.h"
#include "IOInput.h"
#include "main.h"

IOInput IO;

RTC_DATA_ATTR uint16_t ADC_Low;
RTC_DATA_ATTR uint16_t ADC_HIGH;
RTC_DATA_ATTR bool last;

IOInput::IOInput()
{
}

void IOInput::init()
{
    pinMode(ADC_IN, ANALOG);
    pinMode(Open_IN, INPUT_PULLUP);
    pinMode(Close_IN, INPUT_PULLUP);
    pinMode(Battery_IN, ANALOG);
}

uint16_t IOInput::readADC()
{
    return analogRead(ADC_IN);
}

bool IOInput::readOpen()
{
    if (digitalRead(Open_IN) == LOW)
    {
        while (digitalRead(Open_IN) == LOW)
        {
            delay(10);
        }
        return true;
    }
    return false;
}
bool IOInput::readClose()
{
    if (digitalRead(Close_IN) == LOW)
    {
        delay(10);
        if (digitalRead(Close_IN) == LOW)
        {
            Serial.println("Close Down");
            return true;
        }
        Serial.println("Close Up");
        return false;
    }
#ifdef DEBUG
    Serial.println("Close Up");
#endif
    return false;
}
bool IOInput::isClose()
{
    uint16_t temp = readADC();
#ifdef DEBUG
    Serial.printf("ADC:%d\n", temp);
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

uint16_t IOInput::readBattery()
{
    return analogRead(Battery_IN);
}