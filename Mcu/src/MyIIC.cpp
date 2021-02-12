#include "Arduino.h"
#include "MyIIC.h"
#include "main.h"

MyIIC::MyIIC(gpio_num_t SDA, gpio_num_t SCL, i2c_port_t NUM)
{
    I2C_MASTER_NUM = NUM;
    i2c_config_t conf;
    conf.mode = I2C_MODE_MASTER;
    conf.sda_io_num = SDA;
    conf.sda_pullup_en = GPIO_PULLUP_ENABLE;
    conf.scl_io_num = SCL;
    conf.scl_pullup_en = GPIO_PULLUP_ENABLE;
    conf.master.clk_speed = 1000000;
    i2c_param_config(I2C_MASTER_NUM, &conf);
    i2c_driver_install(I2C_MASTER_NUM, conf.mode, 0, 0, 0);
}

void MyIIC::_write(uint8_t reg, uint8_t address)
{
    i2c_cmd_handle_t cmd = i2c_cmd_link_create();
    i2c_master_start(cmd);
    i2c_master_write_byte(cmd, address | I2C_MASTER_WRITE, true);
    i2c_master_write_byte(cmd, reg, true);
    i2c_master_stop(cmd);
    i2c_master_cmd_begin(I2C_MASTER_NUM, cmd, 10 / portTICK_RATE_MS);
    i2c_cmd_link_delete(cmd);
}

void MyIIC::_write(uint8_t *reg, uint32_t size1, uint8_t address)
{
    i2c_cmd_handle_t cmd = i2c_cmd_link_create();
    i2c_master_start(cmd);
    i2c_master_write_byte(cmd, address | I2C_MASTER_WRITE, true);
    i2c_master_write(cmd, reg, size1, true);
    i2c_master_stop(cmd);
    i2c_master_cmd_begin(I2C_MASTER_NUM, cmd, 10 / portTICK_RATE_MS);
    i2c_cmd_link_delete(cmd);
}

void MyIIC::write(uint8_t reg, uint8_t address, uint8_t *data, uint32_t size)
{
    i2c_cmd_handle_t cmd = i2c_cmd_link_create();
    i2c_master_start(cmd);
    i2c_master_write_byte(cmd, address | I2C_MASTER_WRITE, true);
    i2c_master_write_byte(cmd, reg, true);
    for (uint32_t a = 0; a < size; a++)
    {
        i2c_master_write_byte(cmd, data[a], true);
    }
    i2c_master_stop(cmd);
    i2c_master_cmd_begin(I2C_MASTER_NUM, cmd, 10 / portTICK_RATE_MS);
    i2c_cmd_link_delete(cmd);
}

void MyIIC::writeBit(uint8_t reg, uint8_t address, uint8_t data)
{
    i2c_cmd_handle_t cmd = i2c_cmd_link_create();
    i2c_master_start(cmd);
    i2c_master_write_byte(cmd, address | I2C_MASTER_WRITE, true);
    i2c_master_write_byte(cmd, reg, true);
    i2c_master_write_byte(cmd, data, true);
    i2c_master_stop(cmd);
    i2c_master_cmd_begin(I2C_MASTER_NUM, cmd, 10 / portTICK_RATE_MS);
    i2c_cmd_link_delete(cmd);
}

void MyIIC::_read(uint8_t address, uint8_t *data, uint32_t size)
{
    i2c_cmd_handle_t cmd = i2c_cmd_link_create();
    i2c_master_start(cmd);
    i2c_master_write_byte(cmd, address | I2C_MASTER_READ, true);
    for (uint32_t a = 0; a < size; a++)
    {
        i2c_master_read_byte(cmd, data + a, I2C_MASTER_ACK);
    }
    i2c_master_stop(cmd);
    i2c_master_cmd_begin(I2C_MASTER_NUM, cmd, 0);
    i2c_cmd_link_delete(cmd);
}

uint8_t MyIIC::_read(uint8_t address)
{
    uint8_t data;
    i2c_cmd_handle_t cmd = i2c_cmd_link_create();
    i2c_master_start(cmd);
    i2c_master_write_byte(cmd, address | I2C_MASTER_READ, true);
    i2c_master_read_byte(cmd, &data, I2C_MASTER_ACK);
    i2c_master_stop(cmd);
    i2c_master_cmd_begin(I2C_MASTER_NUM, cmd, 0);
    i2c_cmd_link_delete(cmd);
    return data;
}

void MyIIC::read(uint8_t reg, uint8_t address, uint8_t *data, uint32_t size)
{
    _write(reg, address);
    _read(address, data, size);
}

void MyIIC::read(uint8_t *reg, uint32_t size1, uint8_t address, uint8_t *data, uint32_t size)
{
    _write(reg, size1, address);
    _read(address, data, size);
}

uint8_t MyIIC::readBit(uint8_t reg, uint8_t address)
{
    _write(reg, address);
    return _read(address);
}
