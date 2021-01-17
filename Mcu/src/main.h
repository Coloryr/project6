#ifndef MAIN_H
#define MAIN_H

#include "Arduino.h"

extern uint8_t mode;
extern bool Bluetooth_State;
extern bool NetWork_State;

extern class MyBLE *BLE;
extern class Servo *ThisServo;
extern class NBIoT *IoT;

#endif