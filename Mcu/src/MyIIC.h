#ifndef MYIIC_h
#define MYIIC_h

#include "Arduino.h"
#include "main.h"
#include "driver/i2c.h"

class MyIIC
{
private:
    i2c_port_t I2C_MASTER_NUM;
    void _write(uint8_t reg, uint8_t address);
    void _write(uint8_t *reg, uint32_t size1, uint8_t address);
    void _read(uint8_t address, uint8_t *data, uint32_t size);
    uint8_t _read(uint8_t address);

public:
    MyIIC(gpio_num_t SDA, gpio_num_t SCL, i2c_port_t NUM);
    void write(uint8_t reg, uint8_t address, uint8_t *data, uint32_t size);
    void writeBit(uint8_t reg, uint8_t address, uint8_t data);
    void read(uint8_t reg, uint8_t address, uint8_t *data, uint32_t size);
    void read(uint8_t *reg, uint32_t size1, uint8_t address, uint8_t *data, uint32_t size);
    uint8_t readBit(uint8_t reg, uint8_t address);
};

#endif