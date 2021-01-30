#ifndef VL53L0_h
#define VL53L0_h

#include "Arduino.h"
#include "main.h"

#define VL53L0_A 15
#define VL53L0_B 5

#define VL53L0X_REG_IDENTIFICATION_MODEL_ID 0xc0
#define VL53L0X_REG_IDENTIFICATION_REVISION_ID 0xc2
#define VL53L0X_REG_PRE_RANGE_CONFIG_VCSEL_PERIOD 0x50
#define VL53L0X_REG_FINAL_RANGE_CONFIG_VCSEL_PERIOD 0x70
#define VL53L0X_REG_SYSRANGE_START 0x00
#define VL53L0X_REG_RESULT_INTERRUPT_STATUS 0x13
#define VL53L0X_REG_RESULT_RANGE_STATUS 0x14

#define VL53L0X_Add 0x29

#define VL53L0X_STOP 0x00
#define VL53L0X_START 0x01

extern uint16_t maxsize;

class VL53L0
{
private:
    int en;
    bool ok;
    char index;
    uint8_t buff[12];
    uint8_t status;
    uint16_t count[3];
    void start();
    void close();

public:
    VL53L0(int arg0, char arg1);
    void check();
    bool isok();
    bool isready();
    void update();
    char getindex();
};

#endif