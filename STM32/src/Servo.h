#ifndef SERVO_h
#define SERVO_h

#include "stm32f1xx_hal.h"

#define Freq 50      // 频率(20ms周期)

extern uint8_t openset;
extern uint8_t closeset;

class Servo
{
private:
    void setServo(uint8_t degree);

public:
    void open();
    void close();
};

#endif