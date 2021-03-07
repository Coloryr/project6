#ifndef SERVO_h
#define SERVO_h

#include "stm32f1xx_hal.h"

#define Channel 8    // 通道(高速通道（0 ~ 7）由80MHz时钟驱动，低速通道（8 ~ 15）由 1MHz 时钟驱动。)
#define Freq 50      // 频率(20ms周期)
#define Resolution 8 // 分辨率

extern uint8_t openset;
extern uint8_t closeset;

class Servo
{
private:
    void setServo(uint8_t degree);

public:
    Servo();
    void open();
    void close();
};

#endif