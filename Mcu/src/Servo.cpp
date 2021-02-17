#include "Servo.h"
#include "main.h"

uint8_t openset;
uint8_t closeset;

Servo *ThisServo;
Servo::Servo()
{
    ledcSetup(Channel, Freq, Resolution); // 设置通道
    ledcAttachPin(ServoIO, Channel);      // 将通道与对应的引脚连接
}

void Servo::open()
{
#ifdef DEBUG
    Serial.println("Open Trash");
#endif
    setServo(openset);
}

void Servo::close()
{
#ifdef DEBUG
    Serial.println("Close Trash");
#endif
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
