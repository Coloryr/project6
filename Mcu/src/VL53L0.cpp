#include "VL53L0.h"
#include "MyIIC.h"
#include "main.h"
#include "tools.h"

VL53L0 *VL53L0A;
VL53L0 *VL53L0B;

VL53L0::VL53L0(int arg0, char arg1)
{
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
    IIC->WriteBit(VL53L0X_REG_SYSRANGE_START, VL53L0X_Add, VL53L0X_START);
    delay(200);
    start();
    uint8_t data = IIC->ReadBit(VL53L0X_REG_IDENTIFICATION_MODEL_ID, VL53L0X_Add);
    close();
    ok = data == 0xEE;
    if (ok)
    {
        Serial.printf("VL53L0:%c start done\n", index);
    }
    else
    {
        Serial.printf("VL53L0:%c start fail\n", index);
    }
}

bool VL53L0::isok()
{
    return ok;
}

void VL53L0::start()
{
    digitalWrite(en, HIGH);
    delay(10);
    IIC->WriteBit(VL53L0X_REG_SYSRANGE_START, VL53L0X_Add, VL53L0X_START);
}

bool VL53L0::isready()
{
    int times = 0;
    for (;;)
    {
        delay(10);
        uint8_t val = IIC->ReadBit(VL53L0X_REG_RESULT_RANGE_STATUS, VL53L0X_Add);
        if (val & 0x01)
            return true;
        times++;
        if (times > 10)
            return false;
    }
}

void VL53L0::close()
{
    IIC->WriteBit(VL53L0X_REG_SYSRANGE_START, VL53L0X_Add, VL53L0X_STOP);
    delay(10);
    digitalWrite(en, LOW);
}

void VL53L0::update()
{
    start();
    delay(20);
    if (!isready())
    {
        Serial.printf("VL53L0:%c No ready\n", index);
        close();
        return;
    }
    IIC->Read(VL53L0X_REG_RESULT_RANGE_STATUS, VL53L0X_Add, buff, 12);
    count[0] = makeuint16(buff[7], buff[6]);
    count[1] = makeuint16(buff[9], buff[8]);
    count[2] = makeuint16(buff[11], buff[10]);
    status = ((buff[0] & 0x78) >> 3);
    Serial.printf("VL53L0:%c ambient count = %4d signal count = %4d distance = %4d status = %d \n", index, count[0], count[1], count[2], status);
}

char VL53L0::getindex()
{
    return index;
}