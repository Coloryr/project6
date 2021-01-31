#ifndef SERVO_h
#define SERVO_h

#include "Arduino.h"
#include "main.h"

#define ServoIO 18
#define Channel 8       // 通道(高速通道（0 ~ 7）由80MHz时钟驱动，低速通道（8 ~ 15）由 1MHz 时钟驱动。)
#define Freq 50         // 频率(20ms周期)
#define Resolution 8    // 分辨率

class Servo
{
private:
public:
    Servo();
    void SetServo(uint8_t degree);
};

#endif