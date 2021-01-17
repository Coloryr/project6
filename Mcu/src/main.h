#ifndef MAIN_H
#define MAIN_H

#include "Arduino.h"

extern uint8_t mode;
extern uint8_t Bluetooth_State;
extern uint8_t NetWork_State;

extern class MyBLE *BLE;
extern class Servo *ThisServo;
extern class NBIoT *IoT;

#endif