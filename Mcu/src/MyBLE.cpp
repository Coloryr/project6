#include "MyBLE.h"

static BLEUUID serviceUUID("4fafc201-1fb5-459e-8fcc-c5c9c331914b");
static BLEUUID charUUID("beb5483e-36e1-4688-b7f5-ea07361b26a8");

MyBLE *BLE;
boolean ClientDoScan = false;
boolean ClientConnect = false;
BLEAdvertisedDevice *ClientDevice = NULL;

//Client
class ClientCall : public BLEClientCallbacks
{
    void onConnect(BLEClient *pclient)
    {
    }

    void onDisconnect(BLEClient *pclient)
    {
        ClientDoScan = true;
        ClientConnect = false;
        Serial.println("onDisconnect");
    }
};

void ClientNotifyCallback(
    BLERemoteCharacteristic *pBLERemoteCharacteristic,
    uint8_t *pData,
    size_t length,
    bool isNotify)
{
    Serial.print("Notify callback for characteristic ");
    Serial.print(pBLERemoteCharacteristic->getUUID().toString().c_str());
    Serial.print(" of data length ");
    Serial.println(length);
    Serial.print("data: ");
    Serial.println((char *)pData);
}

class ClientSearchCall : public BLEAdvertisedDeviceCallbacks
{
    void onResult(BLEAdvertisedDevice advertisedDevice)
    {
        Serial.print("BLE Advertised Device found: ");
        Serial.println(advertisedDevice.toString().c_str());
        if (advertisedDevice.haveServiceUUID() && advertisedDevice.isAdvertisingService(serviceUUID))
        {
            BLEDevice::getScan()->stop();

            ClientDevice = new BLEAdvertisedDevice(advertisedDevice);
            ClientDoScan = false;
        }
    }
};

//Server
class ServerCall : public BLEServerCallbacks
{
    void onConnect(BLEServer *pServer){};

    void onDisconnect(BLEServer *pServer)
    {
        Serial.println("BLE Advertised Device onDisconnect");
    }
};

class ServerCharacteristicCall : public BLECharacteristicCallbacks
{
    void onWrite(BLECharacteristic *pCharacteristic)
    {
        Serial.print("BLE Advertised Device: ");
        Serial.println(pCharacteristic->toString().c_str());
        Serial.print("Data: ");
        std::string rxValue = pCharacteristic->getValue();
        if (rxValue.length() > 0)
        {
            Serial.println(rxValue.c_str());
        }
    }
};

MyBLE::MyBLE(BLEType type)
{
    // 初始化蓝牙设备
    BLEDevice::init("Coloryr");
    // Bluetooth_State = false;
    if (type == Server)
    {
        BLEServer *pServer = BLEDevice::createServer();
        pServer->setCallbacks(new ServerCall());
        BLEService *pService = pServer->createService(serviceUUID);
        BLECharacteristic *ServerRxCharacteristic = pService->createCharacteristic(
            charUUID,
            BLECharacteristic::PROPERTY_READ |
                BLECharacteristic::PROPERTY_WRITE |
                BLECharacteristic::PROPERTY_NOTIFY |
                BLECharacteristic::PROPERTY_INDICATE);
        ServerRxCharacteristic->addDescriptor(new BLE2902());
        ServerRxCharacteristic->setCallbacks(new ServerCharacteristicCall());

        pService->start();

        BLEAdvertising *pAdvertising = BLEDevice::getAdvertising();
        pAdvertising->addServiceUUID(serviceUUID);
        pAdvertising->setScanResponse(true);
        pAdvertising->setMinPreferred(0x0);

        BLEDevice::startAdvertising();
        pServer->startAdvertising();

        Serial.println("BLE Server Start");
        Task = &MyBLE::TickServer;
        // Bluetooth_State = true;
    }
    else
    {
        pBLEScan = BLEDevice::getScan();
        pBLEScan->setAdvertisedDeviceCallbacks(new ClientSearchCall());
        pBLEScan->setInterval(1349);
        pBLEScan->setWindow(449);
        pBLEScan->setActiveScan(true);
        pBLEScan->start(5);
        ClientDoScan = true;

        Serial.println("BLE Search Start");
        Task = &MyBLE::TickClient;
        // Bluetooth_State = true;
    }
}

void MyBLE::Tick()
{
    (this->*Task)();
}

void MyBLE::TickServer()
{
    
}

void MyBLE::TickClient()
{
    if (ClientDoScan)
    {
        pBLEScan->start(0);
    }
    else if (!ClientConnect)
    {
        Serial.println("BLE Connecting");
        pClient = BLEDevice::createClient();
        pClient->setClientCallbacks(new ClientCall());
        pClient->connect(ClientDevice);
        ClientRemoteService = pClient->getService(serviceUUID);
        if (ClientRemoteService == NULL)
        {
            Serial.print("Failed to find our service UUID: ");
            Serial.println(serviceUUID.toString().c_str());
            pClient->disconnect();
            ClientDoScan = true;
            return;
        }
        ClientRemoteCharacteristic = ClientRemoteService->getCharacteristic(charUUID);
        if (ClientRemoteCharacteristic == NULL)
        {
            Serial.print("Failed to find our characteristic UUID: ");
            Serial.println(charUUID.toString().c_str());
            pClient->disconnect();
            ClientDoScan = true;
            return;
        }

        if (ClientRemoteCharacteristic->canNotify())
            ClientRemoteCharacteristic->registerForNotify(ClientNotifyCallback);

        Serial.println("BLE Connected");
        ClientConnect = true;
    }
    else
    {
        if (ClientRemoteCharacteristic->canWrite())
        {
            Serial.println("Write");
            ClientRemoteCharacteristic->writeValue("test");
        }
    }
    delay(1000);
}