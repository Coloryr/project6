#include "NBIoT.h"
#include "main.h"
#include "IOInput.h"
#include "WString.h"
#include "stdio.h"

NBIoT IoT;

uint8_t UUID[16];
uint8_t User[16];
uint8_t Pass[16];

uint8_t IP[4];
uint16_t Port;

const String TopicTrashServer = "trash/server";
const String TopicTrashClient = "trash/client";

String SelfTopic;
String SelfUUID;

uint8_t SIM[20];

String X;
String Y;
String Time_YMD;
String Time_HMS;

bool ok;
bool card;
bool socket;
bool online;
bool mqtt;

//定义_sys_exit()以避免使用半主机模式
void _sys_exit(int x)
{
    x = x;
}
//重定义fputc函数
int fputc(int ch, FILE *f)
{
    while ((USART1->SR & 0X40) == 0)
        ; //循环发送,直到发送完毕
    USART1->DR = (uint8_t)ch;
    return ch;
}

char USART_RX_BUF[USART_REC_LEN]; //接收缓冲,最大USART_REC_LEN个字节.
//接收状态
//bit15，	接收完成标志
//bit14，	接收到0x0d
//bit13~0，	接收到的有效字节数目
uint16_t USART_RX_STA = 0; //接收状态标记

uint8_t aRxBuffer[RXBUFFERSIZE]; //HAL库使用的串口接收缓冲

void HAL_UART_RxCpltCallback(UART_HandleTypeDef *huart)
{
    if (huart->Instance == USART2) //如果是串口2
    {
        if ((USART_RX_STA & 0x8000) == 0) //接收未完成
        {
            if (USART_RX_STA & 0x4000) //接收到了0x0d
            {
                if (aRxBuffer[0] != 0x0a)
                    USART_RX_STA = 0; //接收错误,重新开始
                else
                    USART_RX_STA |= 0x8000; //接收完成了
            }
            else //还没收到0X0D
            {
                if (aRxBuffer[0] == 0x0d)
                    USART_RX_STA |= 0x4000;
                else
                {
                    USART_RX_BUF[USART_RX_STA & 0X3FFF] = aRxBuffer[0];
                    USART_RX_STA++;
                    if (USART_RX_STA > (USART_REC_LEN - 1))
                        USART_RX_STA = 0; //接收数据错误,重新开始接收
                }
            }
        }
    }
}
//串口1中断服务程序
void USART1_IRQHandler(void)
{
    uint32_t timeout = 0;
    uint32_t maxDelay = 0x1FFFF;

    HAL_UART_IRQHandler(&huart2); //调用HAL库中断处理公用函数

    timeout = 0;
    while (HAL_UART_GetState(&huart2) != HAL_UART_STATE_READY) //等待就绪
    {
        timeout++; ////超时处理
        if (timeout > maxDelay)
            break;
    }

    timeout = 0;
    while (HAL_UART_Receive_IT(&huart2, (uint8_t *)aRxBuffer, RXBUFFERSIZE) != HAL_OK) //一次处理完成之后，重新开启中断并设置RxXferCount为1
    {
        timeout++; //超时处理
        if (timeout > maxDelay)
            break;
    }
}

void flush()
{
    for (uint8_t a = 0; a < USART_REC_LEN; a++)
    {
        USART_RX_BUF[a] = 0x00;
    }
}

void println(const char *temp)
{
    String data(temp);
    data.concat('\n');
    HAL_UART_Transmit(&huart2, (uint8_t *)data.c_str(), data.length(), 1000);
    while (__HAL_UART_GET_FLAG(&huart2, UART_FLAG_TC) != SET)
        ;
}

void println()
{
    uint8_t data = '\n';
    HAL_UART_Transmit(&huart2, &data, 1, 1000);
    while (__HAL_UART_GET_FLAG(&huart2, UART_FLAG_TC) != SET)
        ;
}

String readString()
{
    return String(USART_RX_BUF);
}

void NBIoT::test()
{
    if (mqtt)
    {
#ifdef DEBUG
        Serial.println("连接测试");
#endif
        flush();
        println("AT+QMTCONN?");
        HAL_Delay(300);
        String data = readString();
        data.trim();
        if (!data.startsWith("+QMTCONN: 0,3"))
        {
            mqtt = false;
        }
        else
        {
            mqtt = true;
        }

        return;
    }
}

void NBIoT::tick()
{
    if (USART_RX_STA & 0x8000)
    {
        String data = readString();
        data.trim();
#ifdef DEBUG
        Serial.printf("收到数据:%s\n", data.c_str());
#endif
        if (data.startsWith("+QMTRECV: 0,"))
        {
            data.replace("+QMTRECV: 0,", "");
            data = data.substring(3);
            int index = data.indexOf(',');
            String topic = data.substring(0, index - 1);
            String data1 = data.substring(index + 2, data.length() - 1);
#ifdef DEBUG
            Serial.printf("Topic:%s, Data:%s\n", topic.c_str(), data1.c_str());
#endif
            if (topic.equals(SelfTopic))
            {
                if (data1.equals("Up"))
                {
                    timego = 400;
                }
            }
        }
        else if (data.startsWith("+QMTSTAT: 0,1"))
        {
            mqtt = false;
        }
        USART_RX_STA = 0;
    }
}

void NBIoT::init()
{
    ok = false;
    card = false;
    socket = false;
    online = false;
    mqtt = false;
    flush();
    check();
    if (!ok)
        return;
    getCard();
    if (!card)
        return;
    if (getQuality() == 99)
        return;
    checkOnline();
}

void NBIoT::check()
{
    flush();
    println("ATE0");
    HAL_Delay(300);
    String data = readString();
    data.trim();
    if (data.equalsIgnoreCase("OK"))
    {
#ifdef DEBUG
        Serial.println("NB-IoT:初始化成功");
#endif
        ok = true;
    }
    else
    {
#ifdef DEBUG
        Serial.println("NB-IoT:初始化失败");
#endif
        ok = false;
    }
}
void NBIoT::getCard()
{
    flush();
    println("AT+CIMI");
    HAL_Delay(300);
    String data = readString();
    data.trim();
    if (data.equalsIgnoreCase("ERROR"))
    {
#ifdef DEBUG
        Serial.println("NB-IoT:SIM卡错误");
#endif
        card = false;
    }
    else if (data.endsWith("OK"))
    {
#ifdef DEBUG
        Serial.println("NB-IoT:SIM卡正常");
#endif
        for (uint8_t a = 0; a < data.length(); a++)
        {
            SIM[a] = data[a];
            if (data[a] == 13 || data[a] == 10)
            {
#ifdef DEBUG
                Serial.print("SIM号:");
                Serial.write(SIM, a);
                Serial.println();
#endif
                break;
            }
        }
        card = true;
    }
}

void NBIoT::checkOnline()
{
    flush();
    println("AT+CGATT?");
    HAL_Delay(300);
    String data = readString();
    data.trim();
    if (data.equalsIgnoreCase("ERROR"))
    {
#ifdef DEBUG
        Serial.println("NB-IoT:入网失败");
#endif
    }
    else
    {
        if (data.startsWith("+CGATT: 1"))
        {
#ifdef DEBUG
            Serial.println("NB-IoT:入网成功");
#endif
            online = true;
        }
        else
        {
#ifdef DEBUG
            Serial.println("NB-IoT:入网失败");
#endif
            online = false;
        }
    }
}

uint8_t NBIoT::getQuality()
{
    flush();
    println("AT+CESQ");
    HAL_Delay(300);
    String data = readString();
    data.trim();
    if (data.equalsIgnoreCase("ERROR"))
    {
#ifdef DEBUG
        Serial.println("NB-IoT:没有信号");
#endif
        return 99;
    }
    else
    {
        if (data.startsWith("+CESQ:"))
        {
            data.remove(0, 7);
            int local = data.indexOf(',');
            String data1 = data.substring(0, local);
            uint8_t quality = data1.toInt();
#ifdef DEBUG
            Serial.printf("NB-IoT:信号强度:%d\n", quality);
#endif
            return quality;
        }
    }
    return 99;
}

void NBIoT::setGnssOpen(bool open)
{
#ifdef DEBUG
    Serial.printf("NB-IoT:GNSS模式设置为:%d\n", open);
#endif
    if (!open)
    {
        println("AT+QGNSSC=0");
        return;
    }
    flush();
    println("AT+QGNSSC?");
    HAL_Delay(500);
    String data = readString();
    data.trim();
    if (!data.startsWith("+QGNSSC: 1"))
    {
        println("AT+QGNSSC=1");
        data = String(USART_RX_BUF);
        data.trim();
        if (data.endsWith("OK"))
        {
            println("AT+QGNSSAGPS=1");
            return;
        }
    }
    return;
}

bool NBIoT::readGnss()
{
    flush();
    println("AT+QGNSSRD=\"NMEA/RMC\"");
    HAL_Delay(200);
    String data = readString();
    data.trim();
    if (!data.startsWith("+QGNSSRD: $GNRMC,"))
    {
#ifdef DEBUG
        Serial.println("无效的定位");
#endif
        return false;
    }
    data.replace("+QGNSSRD: $GNRMC,", "");
    if (data[0] == ',')
    {
#ifdef DEBUG
        Serial.println("无效的定位");
#endif
        return false;
    }
    Time_HMS = data.substring(0, 9);
    data = data.substring(10);
    if (data[0] == 'V')
    {
#ifdef DEBUG
        Serial.println("无效的定位");
#endif
        return false;
    }
    else
    {
#ifdef DEBUG
        Serial.println("有效的定位");
#endif
        data = data.substring(2);
        X = data.substring(0, 9);
        data = data.substring(12);
        Y = data.substring(0, 10);
        data = data.substring(20);
        Time_YMD = data.substring(0, 6);
    }
#ifdef DEBUG
    Serial.printf("当前时间:%s, %s\n", Time_YMD.c_str(), Time_HMS.c_str());
    Serial.printf("当前坐标:%s, %s\n", X.c_str(), Y.c_str());
#endif
    return true;
}

bool NBIoT::isOK()
{
    return ok;
}

bool NBIoT::haveCard()
{
    return card;
}

bool NBIoT::isOnline()
{
    return online;
}

bool NBIoT::isMqtt()
{
    return mqtt;
}
bool NBIoT::isSocket()
{
    return socket;
}

void NBIoT::startSocket()
{
    if (!ok || !card || !online)
        return;
}
void NBIoT::startMqtt()
{
    if (!ok || !card || !online)
        return;
#ifdef DEBUG
    Serial.println("正在断开MQTT服务器");
#endif
    flush();
    println("AT+QMTCLOSE=0");
    HAL_Delay(300);
    flush();
    printf("AT+QMTOPEN=0,\"%d.%d.%d.%d\",%d", IP[0], IP[1], IP[2], IP[3], Port);
    println();
    HAL_Delay(10000);
    String data = readString();
    data.trim();
    if (!data.endsWith("+QMTOPEN: 0,0") && !data.endsWith("+QMTSTAT: 0,3"))
    {
#ifdef DEBUG
        Serial.println("MQTT服务器连接失败");
        Serial.println(data.c_str());
#endif
        mqtt = false;
        return;
    }
#ifdef DEBUG
    Serial.println("MQTT服务器已连接");
#endif
    SelfUUID = String("");
    for (uint8_t a = 0; a < 16; a++)
    {
        SelfUUID += (char)UUID[a];
    }
    flush();
    printf("AT+QMTCONN=0,\"%s\",\"%s\",\"%s\"", SelfUUID.c_str(), User, Pass);
    println();
    HAL_Delay(10000);
    data = readString();
    data.trim();
    if (!data.endsWith("+QMTCONN: 0,0,0"))
    {
#ifdef DEBUG
        Serial.println("MQTT服务器认证失败");
        Serial.println(data.c_str());
#endif
        mqtt = false;
        return;
    }
#ifdef DEBUG
    Serial.println("MQTT服务器认证成功");
#endif
    SelfTopic = TopicTrashServer + "/";
    for (uint8_t a = 0; a < 16; a++)
    {
        SelfTopic += (char)UUID[a];
    }
    // printf("AT+QMTSUB=0,1,\"%s\",2", TopicTrashServer.c_str());
    // println();
    // delay(5000);
    printf("AT+QMTSUB=0,2,\"%s\",2", SelfTopic.c_str());
    println();
    flush();
    mqtt = true;
    SendOnce = true;
}
void NBIoT::send()
{
    if (!ok || !card || !online)
        return;
    if (!socket && !mqtt)
        return;
    if (mqtt)
    {
#ifdef DEBUG
        Serial.printf("AT+QMTPUB=0,1,2,0,\"%s\",\"%s,%s,%s,%s,%s,%d,%d,%d,%d\"\n",
                      TopicTrashClient.c_str(),
                      SelfUUID.c_str(),
                      X.c_str(), Y.c_str(),
                      Time_YMD.c_str(), Time_HMS.c_str(),
                      Close, IO.readBattery(), State, Capacity);
#endif
        flush();
        printf("AT+QMTPUB=0,1,2,0,\"%s\",\"%s,%s,%s,%s,%s,%d,%d,%d,%d\"",
               TopicTrashClient.c_str(),
               SelfUUID.c_str(),
               X.c_str(), Y.c_str(),
               Time_YMD.c_str(), Time_HMS.c_str(),
               Close, IO.readBattery(), State, Capacity);
        println();
    }
}
void NBIoT::sendSIM()
{
    if (!ok || !card || !online)
        return;
    if (!socket && !mqtt)
        return;
    if (mqtt)
    {
#ifdef DEBUG
        Serial.printf(" AT+QMTPUB=0,1,2,0,\"%s\",\"%s,%s\"\n",
                      TopicTrashClient.c_str(),
                      SelfUUID.c_str(), SIM);
#endif
        flush();
        printf(" AT+QMTPUB=0,1,2,0,\"%s\",\"%s,%s\"",
               TopicTrashClient.c_str(),
               SelfUUID.c_str(), SIM);
        println();
    }
}

void NBIoT::sleep()
{
    if (!ok)
        return;
#ifdef DEBUG
    Serial.println("NB-IoT:进入低功耗");
#endif
    println("AT+QSCLK=2");
    println("AT+QNBIOTEVENT=1,1");
    flush();
}