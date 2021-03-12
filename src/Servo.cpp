#include "Servo.h"
#include "main.h"
#include "pin.h"

uint8_t openset;
uint8_t closeset;

Servo ThisServo;

void Servo::open()
{
#ifdef DEBUG
    Serial.println("Open Trash");
#endif
    HAL_GPIO_WritePin(GPIOB, ServoPower, GPIO_PIN_RESET);
    HAL_Delay(20);
    HAL_GPIO_WritePin(GPIOB, ServoPower, GPIO_PIN_SET);
    setServo(openset);
}

void Servo::close()
{
#ifdef DEBUG
    Serial.println("Close Trash");
#endif
    HAL_GPIO_WritePin(GPIOB, ServoPower, GPIO_PIN_RESET);
    HAL_Delay(20);
    HAL_GPIO_WritePin(GPIOB, ServoPower, GPIO_PIN_SET);
    setServo(closeset);
}

void Servo::setServo(uint8_t data)
{
    if (data < 0)
        data = 0;
    if (data > 180)
        data = 180;
    float temp;
    temp = (1.0 / 9.0) * data + 5.0; //占空比值 = 1/9 * 角度 + 5
    HAL_TIM_Base_Start(&htim1);
    HAL_TIM_PWM_Start(&htim1, TIM_CHANNEL_1);
    __HAL_TIM_SetCompare(&htim1, TIM_CHANNEL_1, temp);
#ifdef DEBUG
    Serial.printf("PWM:%d, data:%d\n", temp, data);
#endif
}
