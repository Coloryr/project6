#include "IOInput.h"
#include "main.h"
#include "pin.h"

IOInput IO;

uint16_t ADC_Low;
uint16_t ADC_HIGH;
bool last;

uint16_t analogRead(ADC_HandleTypeDef adc)
{
    uint32_t AD_Value = 0;
    uint32_t Value[22];
    for (uint8_t n = 0; n < 22; n++)
    {
        //取22个值做滤波用
        HAL_ADC_Start(&adc);
        HAL_ADC_PollForConversion(&adc, 10); //等待转换完成，第二个参数表示超时时间，单位ms
        if (HAL_IS_BIT_SET(HAL_ADC_GetState(&adc), HAL_ADC_STATE_REG_EOC))
        {
            Value[n] = HAL_ADC_GetValue(&adc);
            AD_Value += Value[n];
        }
    }
    uint32_t max = Value[0];
    uint32_t min = Value[0];
    for (uint8_t n = 0; n < 22; n++) //取最大值、最小值
    {
        max = (Value[n] < max) ? max : Value[n];
        min = (min < Value[n]) ? min : Value[n];
    }
    return ((AD_Value - max - min) / 20); //这里我做了个去掉最大最小值后，取均值的软件滤波
}

uint16_t IOInput::readADC()
{
    return analogRead(hadc2);
}

bool IOInput::readOpen()
{
    if (HAL_GPIO_ReadPin(GPIOB, Open_IN) == GPIO_PIN_RESET)
    {
        while (HAL_GPIO_ReadPin(GPIOB, Open_IN) == GPIO_PIN_RESET)
        {
            HAL_Delay(10);
        }
        return true;
    }
    return false;
}
bool IOInput::readClose()
{
    if (HAL_GPIO_ReadPin(GPIOB, Close_IN) == GPIO_PIN_RESET)
    {
        HAL_Delay(10);
        if (HAL_GPIO_ReadPin(GPIOB, Close_IN) == GPIO_PIN_RESET)
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
    return analogRead(hadc1);
}