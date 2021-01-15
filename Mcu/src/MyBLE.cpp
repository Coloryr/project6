#include "MyBLE.h"

static BLEUUID serviceUUID("4fafc201-1fb5-459e-8fcc-c5c9c331914b");
static BLEUUID charUUID("beb5483e-36e1-4688-b7f5-ea07361b26a8");

MyBLE *BLE;

class MyBLE
{
public:
    BLEServer *pServer = NULL;
    BLEClient *pClient = NULL;

    BLECharacteristic *pTxCharacteristic;
    BLERemoteCharacteristic *pRemoteCharacteristic;

    BLEAdvertisedDevice *myDevice;

    boolean doConnect = false;
    boolean connected = false;
    boolean DoScan = false;
    bool deviceConnected = false;
    char BLEbuf[32] = {0};
    String data = "";

    class MyClientCallback : public BLEClientCallbacks
    {
        void onConnect(BLEClient *pclient)
        {
        }

        void onDisconnect(BLEClient *pclient)
        {
            BLE->deviceConnected = false;
            Serial.println("onDisconnect");
        }
    };

    class ClientSearchCall : public BLEAdvertisedDeviceCallbacks
    {
        void onResult(BLEAdvertisedDevice advertisedDevice)
        {
            if (advertisedDevice.haveServiceUUID() && advertisedDevice.isAdvertisingService(serviceUUID))
            {
                BLEDevice::getScan()->stop();
                BLE->myDevice = new BLEAdvertisedDevice(advertisedDevice);
                BLE->doConnect = true;
                BLE->DoScan = false;
            }
        }
    };

    class MyServerCallbacks : public BLEServerCallbacks
    {
        void onConnect(BLEServer *pServer)
        {
            BLE->deviceConnected = true;
        };

        void onDisconnect(BLEServer *pServer)
        {
            BLE->deviceConnected = false;
        }
    };

    class MyServerCharacteristicCallbacks : public BLECharacteristicCallbacks
    {
        void onWrite(BLECharacteristic *pCharacteristic)
        {
            std::string rxValue = pCharacteristic->getValue();
            if (rxValue.length() > 0)
            {
            }
        }
    };
    MyBLE(BLEType type)
    {
        // 初始化蓝牙设备
        BLEDevice::init("Coloryr");
        if (type == Server)
        {
            // 为蓝牙设备创建服务器
            pServer = BLEDevice::createServer();
            pServer->setCallbacks(new MyServerCallbacks());
            // 基于SERVICE_UUID来创建一个服务
            BLEService *pService = pServer->createService(serviceUUID);
            pTxCharacteristic = pService->createCharacteristic(
                charUUID,
                BLECharacteristic::PROPERTY_NOTIFY);
            pTxCharacteristic->addDescriptor(new BLE2902());
            BLECharacteristic *pRxCharacteristic = pService->createCharacteristic(
                charUUID,
                BLECharacteristic::PROPERTY_WRITE);
            pRxCharacteristic->setCallbacks(new MyServerCharacteristicCallbacks());
            // 开启服务
            pService->start();
            // 开启通知
            pServer->getAdvertising()->start();
            pServer->startAdvertising();
        }
        else
        {
            BLEScan *pBLEScan = BLEDevice::getScan();
            pBLEScan->setAdvertisedDeviceCallbacks(new ClientSearchCall());
            pBLEScan->setInterval(1349);
            pBLEScan->setWindow(449);
            pBLEScan->setActiveScan(true);
            pBLEScan->start(5, false);
        }
    }
    void TickServer()
    {
        if (deviceConnected == 1 & data.length() > 0)
        {
            memset(BLEbuf, 0, 32);
            memcpy(BLEbuf, data.c_str(), 32);
            Serial.println(BLEbuf);

            pTxCharacteristic->setValue(BLEbuf); //收到数据后返回数据
            pTxCharacteristic->notify();
            data = ""; //返回数据后进行清空，否则一直发送data
        }
    }
    void TickClient()
    {
        if (DoScan)
        {
            BLEDevice::getScan()->start(0);
        }
        else
        {
            BLE->pClient = BLEDevice::createClient();
            BLE->pClient->setClientCallbacks(new MyClientCallback());
        }
    }
};