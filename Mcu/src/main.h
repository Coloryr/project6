#ifndef MAIN_H
#define MAIN_H

#include "Arduino.h"

#define DEBUG true

extern uint8_t Capacity;
extern bool Close;
extern uint8_t State;
extern bool SendOnce;
extern uint16_t timego;

extern class Servo ThisServo;
extern class NBIoT IoT;
extern class VL53L0 VL53L0A;
extern class VL53L0 VL53L0B;
extern class EEPROM ThisEEPROM;
extern class IOInput IO;
extern class Upload Up;

uint8_t SumDistance();

#endif