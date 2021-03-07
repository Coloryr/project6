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
    const float deadZone = 6.4; //对应0.5ms（0.5ms/(20ms/256）)
    const float max = 32;       //对应2.5ms
    if (data < 0)
        data = 0;
    if (data > 180)
        data = 180;
    uint32_t temp = (((max - deadZone) / 180) * data + deadZone);

    ledcWrite(Channel, temp); // 输出PWM

#ifdef DEBUG
    Serial.printf("PWM:%d, data:%d\n", temp, data);
#endif
}
