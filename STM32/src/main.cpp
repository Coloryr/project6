#include "main.h"
#include "Servo.h"
#include "NBIoT.h"
#include "VL53L0.h"
#include "EEPROM.h"
#include "IOInput.h"
#include "Upload.h"
#include "pin.h"

ADC_HandleTypeDef hadc1;
ADC_HandleTypeDef hadc2;

I2C_HandleTypeDef hi2c1;
I2C_HandleTypeDef hi2c2;

TIM_HandleTypeDef htim1;

UART_HandleTypeDef huart1;
UART_HandleTypeDef huart2;

bool Close;
bool busy;
bool IsOpen;
bool SendOnce;

bool Init = false;

uint16_t timego = 0;
uint16_t timego1 = 0;
uint8_t State;
//0 ok
//1 初始化
//2 初始化完成
//3 距离传感器错误
//5 快满了
uint8_t Capacity;

void longTask()
{
    timego++;
    timego1++;

    if (!IoT.isOnline())
    {
        busy = true;
        IoT.init();
        HAL_Delay(200);
        IoT.test();
        HAL_Delay(200);
        IoT.sleep();
        busy = false;
    }
    else if (!IoT.isMqtt())
    {
        busy = true;
        IoT.startMqtt();
        HAL_Delay(200);
        IoT.sendSIM();
        HAL_Delay(200);
        IoT.sleep();
        busy = false;
    }

    if (SendOnce)
    {
#ifdef DEBUG
        Serial.println("上传数据");
#endif
        busy = true;
        IoT.setGnssOpen(true);
        HAL_Delay(200);
        uint8_t time = 0;
        for (;;)
        {
            if (IoT.readGnss())
            {
                break;
            }
            time++;
            if (time >= 5)
            {
                IoT.setGnssOpen(true);
                time = 0;
            }
            HAL_Delay(10000);
        }
        HAL_Delay(200);
        IoT.setGnssOpen(false);
        HAL_Delay(200);
        IoT.send();
        HAL_Delay(200);
        IoT.sleep();
        timego = 0;
        timego1 = 0;
        SendOnce = false;
        busy = false;
    }

    if (timego1 > 30)
    {
        timego1 = 0;
        busy = true;
        IoT.test();
        HAL_Delay(200);
        IoT.sleep();
        busy = false;
    }
    if (timego > 360)
    {
        SendOnce = true;
    }
}

void tick()
{
    if (IsOpen)
    {
        ThisServo.close();
        IsOpen = false;
    }

    Close = IO.isClose() && IO.readClose();
    if (!Close)
        return;
    if (VL53L0A->isOK())
    {
        VL53L0A->update();
    }
    if (VL53L0B->isOK())
    {
        VL53L0B->update();
    }
    double sum = 0;
    uint8_t count = 0;
    if (VL53L0A->status == 11)
    {
        if (VL53L0A->count[2] <= Distance)
        {
            double temp = (double)VL53L0A->count[2] / (double)Distance;
            sum += temp * 100;
            count++;
        }
    }
    if (VL53L0B->status == 11)
    {
        if (VL53L0B->count[2] <= Distance)
        {
            double temp = (double)VL53L0B->count[2] / (double)Distance;
            sum += temp * 100;
            count++;
        }
    }
    if (count == 0)
    {
        State = 3;
    }
    else
    {
        Capacity = sum / count;
        if (Capacity < 10)
        {
            State = 5;
            SendOnce = true;
        }
        else
        {
            if (State == 5)
            {
                SendOnce = true;
            }
            State = 0;
        }
    }
}

void Io_Read()
{
    if (IO.readOpen())
    {
        SendOnce = true;
        IsOpen = true;
        ThisServo.open();
    }
}

int main(void)
{
    HAL_Init();
    SystemClock_Config();
    MX_GPIO_Init();
    MX_ADC1_Init();
    MX_ADC2_Init();
    MX_I2C1_Init();
    MX_I2C2_Init();
    MX_TIM1_Init();
    MX_USART1_UART_Init();
    MX_USART2_UART_Init();

    ThisServo.close();
    ThisEEPROM.init();
    VL53L0A = new VL53L0(VL53L0_A, '0');
    VL53L0B = new VL53L0(VL53L0_B, '1');
    VL53L0A->check();
    VL53L0B->check();

    IoT.init();
    State = 2;

    while (1)
    {
        HAL_Delay(1000);
    }
}

void SysTick_Handler(void)
{
    HAL_IncTick();
}

void NMI_Handler(void)
{
}

void HardFault_Handler(void)
{
    while (1)
    {
    }
}

void MemManage_Handler(void)
{
    while (1)
    {
    }
}

void BusFault_Handler(void)
{
    while (1)
    {
    }
}

void UsageFault_Handler(void)
{
    while (1)
    {
    }
}

void SVC_Handler(void)
{
}

void DebugMon_Handler(void)
{
}

void PendSV_Handler(void)
{
}

void Error_Handler(void)
{
    /* USER CODE BEGIN Error_Handler_Debug */
    /* User can add his own implementation to report the HAL error return state */
    __disable_irq();
    while (1)
    {
    }
    /* USER CODE END Error_Handler_Debug */
}
