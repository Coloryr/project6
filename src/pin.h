#ifndef PIN_h
#define PIN_h

#include "stm32f1xx_hal.h"

//PB
#define ADC_IN GPIO_PIN_1 //ADC2
#define Battery_IN GPIO_PIN_0 //ADC1
//PB
#define Open_IN GPIO_PIN_12
#define Close_IN GPIO_PIN_13
//PB
#define I2C_EEPROM_SCL GPIO_PIN_8
#define I2C_EEPROM_SDA GPIO_PIN_9
//PA
#define ServoIO GPIO_PIN_8
//PB
#define ServoPower GPIO_PIN_15

//PB
#define VL53L0_A GPIO_PIN_14
#define VL53L0_B GPIO_PIN_5
//PB
#define I2C_VL53L0_SCL GPIO_PIN_10
#define I2C_VL53L0_SDA GPIO_PIN_11

#endif