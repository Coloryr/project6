#include "VL53L0.h"
#include "MyIIC.h"
#include "main.h"
#include "tools.h"

uint16_t maxsize;

VL53L0 *VL53L0A;
VL53L0 *VL53L0B;
MyIIC *VL53L0IIC = NULL;

VL53L0::VL53L0(int arg0, char arg1)
{
    if (VL53L0IIC == NULL)
    {
        VL53L0IIC = new MyIIC(I2C_VL53L0_SDA, I2C_VL53L0_SCL, I2C_VL53L0_NUM);
    }
    en = arg0;
    index = arg1;
    pinMode(arg0, OUTPUT);
    digitalWrite(en, LOW);
    delay(50);
}

void VL53L0::check()
{
    digitalWrite(en, HIGH);
    delay(20);
    VL53L0IIC->WriteBit(VL53L0X_REG_SYSRANGE_START, VL53L0X_Add, VL53L0X_START);
    delay(200);
    start();
    uint8_t data = VL53L0IIC->ReadBit(VL53L0X_REG_IDENTIFICATION_MODEL_ID, VL53L0X_Add);
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

bool VL53L0::isok()
{
    return ok;
}

void VL53L0::start()
{
    digitalWrite(en, HIGH);
    delay(10);
    VL53L0IIC->WriteBit(VL53L0X_REG_SYSRANGE_START, VL53L0X_Add, VL53L0X_START);
}

bool VL53L0::isready()
{
    int times = 0;
    for (;;)
    {
        delay(10);
        uint8_t val = VL53L0IIC->ReadBit(VL53L0X_REG_RESULT_RANGE_STATUS, VL53L0X_Add);
        if (val & 0x01)
            return true;
        times++;
        if (times > 10)
            return false;
    }
}

void VL53L0::close()
{
    VL53L0IIC->WriteBit(VL53L0X_REG_SYSRANGE_START, VL53L0X_Add, VL53L0X_STOP);
    delay(10);
    digitalWrite(en, LOW);
}

void VL53L0::update()
{
    start();
    delay(20);
    if (!isready())
    {
#ifdef DEBUG
        Serial.printf("VL53L0:%c No ready\n", index);
#endif
        close();
        return;
    }
    VL53L0IIC->Read(VL53L0X_REG_RESULT_RANGE_STATUS, VL53L0X_Add, buff, 12);
    count[0] = makeuint16(buff[7], buff[6]);
    count[1] = makeuint16(buff[9], buff[8]);
    count[2] = makeuint16(buff[11], buff[10]);
    status = ((buff[0] & 0x78) >> 3);
#ifdef DEBUG
    Serial.printf("VL53L0:%c ambient count = %4d signal count = %4d distance = %4d status = %d \n", index, count[0], count[1], count[2], status);
#endif
    close();
}

char VL53L0::getindex()
{
    return index;
}