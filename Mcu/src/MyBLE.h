#ifndef MYBLE_h
#define MYBLE_h

#include "Arduino.h"
#include "main.h"
#include <BLEDevice.h>
#include <BLEServer.h>
#include <BLEUtils.h>
#include <BLE2902.h>

enum BLEType
{
    Server,
    Client
};

class MyBLE
{
private:
    BLERemoteCharacteristic *ClientRemoteCharacteristic = NULL;
    BLERemoteService *ClientRemoteService = NULL;
    BLEClient *pClient = NULL;
    BLEScan *pBLEScan = NULL;
    void (MyBLE::*Task)();
    void TickServer();
    void TickClient();
public:
    MyBLE(BLEType type);
    void Tick();
};

#endif