#include "Upload.h"
#include "main.h"
#include "NBIoT.h"
#include "EEPROM.h"

Upload *Up;

const uint8_t TestPack[9] = {0x87, 0x32, 0xf5, 0xae, 0x1d, 0x91, 0x0f, 0x8e, 0x3f};
const uint8_t ResPack[9] = {0x21, 0x00, 0x4f, 0x56, 0xae, 0xac, 0xe3, 0x76, 0x89};
const uint8_t ReadPack[5] = {0x52, 0x45, 0x41, 0x44, 0x3A};
const uint8_t SetPack[4] = {0x53, 0x45, 0x54, 0x3A};
const uint8_t OKPack[] = {0x4F, 0x4B};

Upload::Upload()
{
    readbuff = new uint8_t[64];
    writebuff = new uint8_t[64];
    open = false;
    reset();
}

void Upload::tick()
{
    if (Serial.available() > 0)
    {
        Serial.readBytes(readbuff, 24);
        check();
    }
}

void Upload::check()
{
    if (readbuff[0] == 0x87)
    {
        for (uint8_t a = 0; a < 9; a++)
        {
            if (TestPack[a] != readbuff[a])
                return;
        }
        open = true;
        sendopen();
    }
    else if (readbuff[0] == 0x52)
    {
        for (uint8_t a = 0; a < 5; a++)
        {
            if (ReadPack[a] != readbuff[a])
                return;
        }
        sendread(readbuff[5]);
    }
    else if (readbuff[0] == 0x53)
    {
        for (uint8_t a = 0; a < 4; a++)
        {
            if (SetPack[a] != readbuff[a])
                return;
        }
        sendwrite(readbuff[4]);
        sendok();
    }
}

void Upload::sendread(uint8_t type)
{
    switch (type)
    {
    case 0:
        buildpack(0);
        for (uint8_t a = 0; a < 16; a++)
        {
            writebuff[a + 6] = UUID[a];
        }
        send(22);
        break;
    case 4:
        buildpack(4);
        writebuff[6] = IP[0];
        writebuff[7] = IP[1];
        writebuff[8] = IP[2];
        writebuff[9] = IP[3];
        writebuff[10] = Port[0];
        writebuff[11] = Port[1];
        send(12);
        break;
    }
}
void Upload::sendwrite(uint8_t type)
{
    switch (type)
    {
    case 0:
        for (uint8_t a = 0; a < 16; a++)
        {
            UUID[a] = readbuff[a + 5];
        }
        ThisEEPROM->saveuuid();
        break;
    case 4:
        IP[0] = readbuff[5];
        IP[1] = readbuff[6];
        IP[2] = readbuff[7];
        IP[3] = readbuff[8];
        Port[0] = readbuff[9];
        Port[1] = readbuff[10];
        ThisEEPROM->saveip();
        break;
    }
    reset();
}
void Upload::buildpack(uint8_t type)
{
    for (uint8_t a = 0; a < 5; a++)
    {
        writebuff[a] = ReadPack[a];
    }
    writebuff[5] = type;
}

void Upload::sendopen()
{
    Serial.write(ResPack, 9);
}

void Upload::sendok()
{
    Serial.write(OKPack, 2);
}

void Upload::send(uint8_t size)
{
    Serial.write(writebuff, size);
    reset();
}

void Upload::reset()
{
    for (int a = 0; a < 64; a++)
    {
        writebuff[a] = 0;
    }
}