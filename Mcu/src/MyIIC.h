#ifndef MYIIC_h
#define MYIIC_h

#include "Arduino.h"
#include "main.h"
#include "driver/i2c.h"

#define I2C_MASTER_SCL_IO GPIO_NUM_2 /*!< gpio number for I2C master clock */
#define I2C_MASTER_SDA_IO GPIO_NUM_4 /*!< gpio number for I2C master data  */
#define I2C_MASTER_NUM I2C_NUM_1     /*!< I2C port number for master dev */
#define I2C_MASTER_TX_BUF_DISABLE 0  /*!< I2C master do not need buffer */
#define I2C_ASTER_RX_BUF_DISABLE 0   /*!< I2C master do not need buffer */
#define I2C_MASTER_FREQ_HZ 1000000   /*!< I2C master clock frequency */

class MyIIC
{
private:
public:
    MyIIC();
    void Write(uint8_t reg, uint8_t address, uint8_t *data, uint32_t size);
    void WriteBit(uint8_t reg, uint8_t address, uint8_t data);
    void Read(uint8_t reg, uint8_t address, uint8_t *data, uint32_t size);
    void Read(uint8_t *reg, uint32_t size1, uint8_t address, uint8_t *data, uint32_t size);
    uint8_t ReadBit(uint8_t reg, uint8_t address);
};

#endif