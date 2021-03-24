#include "Servo.h"
#include "IOInput.h"
#include "main.h"

RTC_DATA_ATTR uint8_t openset;
RTC_DATA_ATTR uint8_t closeset;

Servo ThisServo;
Servo::Servo()
{
}

void Servo::init()
{
    ledcSetup(Channel, Freq, Resolution); // 设置通道
    ledcAttachPin(ServoIO, Channel);      // 将通道与对应的引脚连接
    pinMode(ServoPower, OUTPUT);
}

void Servo::open()
{
    if (IO.readBattery() < 2100)
    {
#ifdef DEBUG
    Serial.println("No Power");
#endif    
        return;
    }
#ifdef DEBUG
    Serial.println("Open Trash");
#endif
    init();
    digitalWrite(ServoPower, LOW);
    delay(20);
    digitalWrite(ServoPower, HIGH);
    setServo(openset);
    delay(1000);
}

void Servo::close()
{
#ifdef DEBUG
    Serial.println("Close Trash");
#endif
    init();
    digitalWrite(ServoPower, LOW);
    delay(20);
    digitalWrite(ServoPower, HIGH);
    setServo(closeset);
    delay(1000);
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
