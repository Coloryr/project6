#ifndef MAIN_H
#define MAIN_H

#include "Arduino.h"

extern uint8_t mode;
extern bool Bluetooth_State;
extern bool NetWork_State;

extern class MyBLE *BLE;
extern class Servo *ThisServo;
extern class NBIoT *IoT;
extern class MyIIC *IIC;
extern class VL53L0 *VL53L0A;
extern class VL53L0 *VL53L0B;
extern class EEPROM* ThisEEPROM;

#endif