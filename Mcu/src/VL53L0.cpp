#include "VL53L0.h"
#include "MyIIC.h"
#include "main.h"

VL53L0 *VL53L0A;
VL53L0 *VL53L0B;

VL53L0::VL53L0(int open)
{
    en = open;
    pinMode(open, OUTPUT);
    digitalWrite(en, LOW);
    delay(50);
}

bool VL53L0::isok()
{
    digitalWrite(en, HIGH);
    delay(20);
    IIC->WriteBit(VL53L0X_REG_SYSRANGE_START, VL53L0X_Add, VL53L0X_START);
    delay(200);
    Start();
    uint8_t data = IIC->ReadBit(VL53L0X_REG_IDENTIFICATION_MODEL_ID, VL53L0X_Add);
    Serial.printf("data:%d\n", data);
    delay(20);
    digitalWrite(en, LOW);
    return data == 0xEE;
}

void VL53L0::Start()
{
    digitalWrite(en, HIGH);
    delay(10);
    for (;;)
    {
        delay(10);
        uint8_t val = IIC->ReadBit(VL53L0X_REG_RESULT_RANGE_STATUS, VL53L0X_Add);
        Serial.printf("data:%d\n", val);
        if (val & 0x01)
            break;
    }
}

void VL53L0::Close()
{
    IIC->WriteBit(VL53L0X_REG_SYSRANGE_START, VL53L0X_Add, VL53L0X_STOP);
    delay(10);
    digitalWrite(en, LOW);
}
