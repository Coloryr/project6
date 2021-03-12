#ifndef MAIN_H
#define MAIN_H

#include "stm32f1xx_hal.h"

// #define DEBUG true
// #define SLEEP true

extern uint8_t Capacity;
extern bool Close;
extern uint8_t State;
extern bool SendOnce;
extern uint16_t timego;

extern ADC_HandleTypeDef hadc1;
extern ADC_HandleTypeDef hadc2;

extern I2C_HandleTypeDef hi2c1;
extern I2C_HandleTypeDef hi2c2;

extern TIM_HandleTypeDef htim1;

extern UART_HandleTypeDef huart1;
extern UART_HandleTypeDef huart2;

extern class Servo ThisServo;
extern class NBIoT IoT;
extern class VL53L0 *VL53L0A;
extern class VL53L0 *VL53L0B;
extern class EEPROM ThisEEPROM;
extern class IOInput IO;
extern class Upload Up;

uint8_t SumDistance();

void Error_Handler(void);

void HAL_TIM_MspPostInit(TIM_HandleTypeDef *htim);
void SystemClock_Config(void);
void MX_GPIO_Init(void);
void MX_ADC1_Init(void);
void MX_ADC2_Init(void);
void MX_I2C1_Init(void);
void MX_I2C2_Init(void);
void MX_TIM1_Init(void);
void MX_USART1_UART_Init(void);
void MX_USART2_UART_Init(void);

typedef union
{
    uint16_t u16;
    uint8_t u8[2];
} Mytow;

#endif