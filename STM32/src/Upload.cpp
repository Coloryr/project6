#include "Upload.h"
#include "main.h"
#include "NBIoT.h"
#include "EEPROM.h"
#include "IOInput.h"
#include "VL53L0.h"
#include "Servo.h"

Upload Up;

uint8_t TestPack[9] = {0x87, 0x32, 0xf5, 0xae, 0x1d, 0x91, 0x0f, 0x8e, 0x3f};
uint8_t ResPack[9] = {0x21, 0x00, 0x4f, 0x56, 0xae, 0xac, 0xe3, 0x76, 0x89};
uint8_t ReadPack[5] = {0x52, 0x45, 0x41, 0x44, 0x3A};
uint8_t SetPack[4] = {0x53, 0x45, 0x54, 0x3A};
uint8_t OKPack[] = {0x4F, 0x4B};

Upload::Upload()
{
    readbuff = new uint8_t[64];
    writebuff = new uint8_t[64];
    open = false;
    reset();
}

void Upload::tick()
{
    if (__HAL_USART_GET_FLAG(&huart1, UART_FLAG_RXNE) != RESET)
    {
        HAL_UART_Receive(&huart1, readbuff, 64, 100);
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
        sendOpen();
    }
    else if (readbuff[0] == 0x52)
    {
        for (uint8_t a = 0; a < 5; a++)
        {
            if (ReadPack[a] != readbuff[a])
                return;
        }
        sendRead(readbuff[5]);
    }
    else if (readbuff[0] == 0x53)
    {
        for (uint8_t a = 0; a < 4; a++)
        {
            if (SetPack[a] != readbuff[a])
                return;
        }
        sendWrite(readbuff[4]);
        sendOK();
    }
}

void Upload::sendRead(uint8_t type)
{
    switch (type)
    {
    case 0:
        buildPack(0);
        for (uint8_t a = 0; a < 16; a++)
        {
            writebuff[a + 6] = UUID[a];
        }
        send(22);
        break;
    case 1:
        buildPack(1);
        Mytow tow;
        tow.u16 = Distance;
        writebuff[6] = tow.u8[0];
        writebuff[7] = tow.u8[1];
        tow.u16 = ADC_Low;
        writebuff[8] = tow.u8[0];
        writebuff[9] = tow.u8[1];
        tow.u16 = ADC_HIGH;
        writebuff[10] = tow.u8[0];
        writebuff[11] = tow.u8[1];
        writebuff[12] = openset;
        writebuff[13] = closeset;
        send(14);
        break;
    case 2:
        buildPack(2);
        writebuff[6] = IP[0];
        writebuff[7] = IP[1];
        writebuff[8] = IP[2];
        writebuff[9] = IP[3];
        tow.u16 = Port;
        writebuff[10] = tow.u8[0];
        writebuff[11] = tow.u8[1];
        send(12);
        break;
    case 3:
        buildPack(3);
        tow.u16 = IO.readADC();
        writebuff[6] = tow.u8[1];
        writebuff[7] = tow.u8[0];
        VL53L0A->update();
        VL53L0B->update();
        tow.u16 = VL53L0A->count[2];
        writebuff[8] = tow.u8[0];
        writebuff[9] = tow.u8[1];
        writebuff[10] = VL53L0A->status;
        tow.u16 = VL53L0B->count[2];
        writebuff[11] = tow.u8[0];
        writebuff[12] = tow.u8[1];
        writebuff[13] = VL53L0B->status;
        send(14);
        break;
    case 4:
        buildPack(4);
        for (uint8_t a = 0; a < 16; a++)
        {
            writebuff[a + 6] = User[a];
            writebuff[a + 22] = Pass[a];
        }
        send(38);
        break;
    }
}
void Upload::sendWrite(uint8_t type)
{
    switch (type)
    {
    case 0:
        for (uint8_t a = 0; a < 16; a++)
        {
            UUID[a] = readbuff[a + 5];
        }
        ThisEEPROM.saveUUID();
        break;
    case 1:
        Mytow tow;
        tow.u8[0] = readbuff[5];
        tow.u8[1] = readbuff[6];
        Distance = tow.u16;
        tow.u8[0] = readbuff[7];
        tow.u8[1] = readbuff[8];
        ADC_Low = tow.u16;
        tow.u8[0] = readbuff[9];
        tow.u8[1] = readbuff[10];
        ADC_HIGH = tow.u16;
        openset = readbuff[11];
        closeset = readbuff[12];
        ThisEEPROM.saveSet();
        break;
    case 2:
        IP[0] = readbuff[5];
        IP[1] = readbuff[6];
        IP[2] = readbuff[7];
        IP[3] = readbuff[8];
        tow.u8[0] = readbuff[9];
        tow.u8[1] = readbuff[10];
        Port = tow.u16;
        ThisEEPROM.saveIP();
        break;
    case 4:
        for (uint8_t a = 0; a < 16; a++)
        {
            User[a] = readbuff[5 + a];
            Pass[a] = readbuff[21 + a];
        }
        ThisEEPROM.saveMqtt();
        break;
    }
    reset();
}
void Upload::buildPack(uint8_t type)
{
    for (uint8_t a = 0; a < 5; a++)
    {
        writebuff[a] = ReadPack[a];
    }
    writebuff[5] = type;
}

void Upload::sendOpen()
{
    HAL_UART_Transmit(&huart1, ResPack, 9, 200);
}

void Upload::sendOK()
{
    HAL_UART_Transmit(&huart1, OKPack, 2, 200);
}

void Upload::send(uint8_t size)
{
    HAL_UART_Transmit(&huart1, writebuff, size, 200);
    reset();
}

void Upload::reset()
{
    for (int a = 0; a < 64; a++)
    {
        writebuff[a] = 0;
    }
}