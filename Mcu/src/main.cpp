#include "Arduino.h"
#include "MyBLE.h"
#include "Servo.h"
#include "main.h"
#include "NBIoT.h"
#include "MyIIC.h"
#include "VL53L0.h"
#include "EEPROM.h"
#include "IOInput.h"
#include "Upload.h"

uint8_t mode;
bool Bluetooth_State;
bool NetWork_State;

void TaskUpload(void *data)
{
    Up->tick();
    delay(100);
}

void setup()
{
    Serial.begin(115200);

    ThisServo = new Servo();
    IoT = new NBIoT();
    ThisEEPROM = new EEPROM();
    IO = new IOInput();
    VL53L0A = new VL53L0(VL53L0_A, '0');
    VL53L0B = new VL53L0(VL53L0_B, '1');

    // VL53L0A->check();
    // VL53L0B->check();

    ThisEEPROM->init();

    Up = new Upload();

    // if (NetWork_State)
    // {
    //     BLE = new MyBLE(Server);
    // }
    // else
    // {
    //     BLE = new MyBLE(Client);
    // }
    xTaskCreate(TaskUpload, "Upload", 1024, NULL, 3, NULL);
}

void loop()
{
    // VL53L0A->update();
    // VL53L0B->update();
    IO->isclose();
    delay(2000);
    // BLE->Tick();
    // for (int d = 1; d < 180; d += 1)
    // {
    //     ThisServo->SetServo(d);
    //     Serial.printf("value=%d\n", d);
    //     delay(500);
    // }
}
