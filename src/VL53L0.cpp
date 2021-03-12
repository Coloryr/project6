#include "VL53L0.h"
#include "main.h"
#include "pin.h"

uint16_t Distance;

VL53L0 *VL53L0A;
VL53L0 *VL53L0B;

VL53L0::VL53L0(int arg0, char arg1)
{
    en = arg0;
    index = arg1;
    HAL_GPIO_WritePin(GPIOB, en, GPIO_PIN_RESET);
    HAL_Delay(50);
}

void VL53L0::check()
{
    HAL_GPIO_WritePin(GPIOB, en, GPIO_PIN_SET);
    HAL_Delay(20);
    uint8_t data = VL53L0X_START;
    HAL_I2C_Mem_Write(&hi2c2, VL53L0X_Add, VL53L0X_REG_SYSRANGE_START, I2C_MEMADD_SIZE_8BIT, &data, 2, 1000);
    HAL_Delay(200);
    start();
    HAL_I2C_Mem_Read(&hi2c2, VL53L0X_Add, VL53L0X_REG_IDENTIFICATION_MODEL_ID, I2C_MEMADD_SIZE_8BIT, &data, 1, 1000);
    close();
    ok = data == 0xEE;
#ifdef DEBUG
    if (ok)
    {
        Serial.printf("VL53L0:%c start done\n", index);
    }
    else
    {
        Serial.printf("VL53L0:%c start fail\n", index);
    }
#endif
}

bool VL53L0::isOK()
{
    return ok;
}

void VL53L0::start()
{
    HAL_GPIO_WritePin(GPIOB, en, GPIO_PIN_SET);
    HAL_Delay(10);
    uint8_t data = VL53L0X_START;
    HAL_I2C_Mem_Write(&hi2c2, VL53L0X_Add, VL53L0X_REG_SYSRANGE_START, I2C_MEMADD_SIZE_8BIT, &data, 2, 1000);
}

bool VL53L0::isReady()
{
    int times = 0;
    for (;;)
    {
        HAL_Delay(10);
        uint8_t val;
        HAL_I2C_Mem_Read(&hi2c2, VL53L0X_Add, VL53L0X_REG_RESULT_RANGE_STATUS, I2C_MEMADD_SIZE_8BIT, &val, 1, 1000);
        if (val & 0x01)
            return true;
        times++;
        if (times > 10)
            return false;
    }
}

void VL53L0::close()
{
    uint8_t data = VL53L0X_STOP;
    HAL_I2C_Mem_Write(&hi2c2, VL53L0X_Add, VL53L0X_REG_SYSRANGE_START, I2C_MEMADD_SIZE_8BIT, &data, 2, 1000);
    HAL_Delay(10);
    HAL_GPIO_WritePin(GPIOB, en, GPIO_PIN_RESET);
}

void VL53L0::update()
{
    start();
    HAL_Delay(20);
    if (!isReady())
    {
#ifdef DEBUG
        Serial.printf("VL53L0:%c No ready\n", index);
#endif
        status = 0;
        close();
        return;
    }
    HAL_I2C_Mem_Read(&hi2c2, VL53L0X_Add, VL53L0X_REG_RESULT_RANGE_STATUS, I2C_MEMADD_SIZE_8BIT, buff, 12, 1000);
    Mytow tow;
    tow.u8[1] = buff[6];
    tow.u8[0] = buff[7];
    count[0] = tow.u16;
    tow.u8[1] = buff[8];
    tow.u8[0] = buff[9];
    count[1] = tow.u16;
    tow.u8[1] = buff[10];
    tow.u8[0] = buff[11];
    count[2] = tow.u16;
    status = ((buff[0] & 0x78) >> 3);
    close();
}

char VL53L0::getIndex()
{
    return index;
}