#include "IOInput.h"
#include "main.h"
#include "pin.h"

IOInput IO;

uint16_t ADC_Low; 
uint16_t ADC_HIGH;
bool last;

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
            HAL_Delay(10);
        }
        return true;
    }
    return false;
}
bool IOInput::readClose()
{
    if (digitalRead(Close_IN) == LOW)
    {
        HAL_Delay(10);
        if (digitalRead(Close_IN) == LOW)
        {
            return true;
        }
        return false;
    }
    return false;
}
bool IOInput::isClose()
{
    uint16_t temp = readADC();
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