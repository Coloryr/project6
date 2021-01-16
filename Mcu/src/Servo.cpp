#include "Servo.h"

Servo *ThisServo;
Servo::Servo()
{
    ledcSetup(Channel, Freq, Resolution); // 设置通道
    ledcAttachPin(ServoIO, Channel);          // 将通道与对应的引脚连接
}

void Servo::SetServo(int data)
{
    const float deadZone = 6.4; //对应0.5ms（0.5ms/(20ms/256）)
    const float max = 32;       //对应2.5ms
    if (data < 0)
        data = 0;
    if (data > 180)
        data = 180;
    ledcWrite(Channel, (((max - deadZone) / 180) * data + deadZone)); // 输出PWM
}
