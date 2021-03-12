#ifndef NBIOT_h
#define NBIOT_h

#include "stm32f1xx_hal.h"

#define USART_REC_LEN  			200  	//定义最大接收字节数 200
#define EN_USART1_RX 			1		//使能（1）/禁止（0）串口1接收

extern char  USART_RX_BUF[USART_REC_LEN]; //接收缓冲,最大USART_REC_LEN个字节.末字节为换行符 
extern uint16_t USART_RX_STA;         		//接收状态标记	

#define RXBUFFERSIZE   1 //缓存大小
extern uint8_t aRxBuffer[RXBUFFERSIZE];//HAL库USART接收Buffer

extern uint8_t UUID[16];
extern uint8_t User[16];
extern uint8_t Pass[16];

extern uint8_t IP[4];
extern uint16_t Port;

extern uint8_t SIM[20];

class NBIoT
{
private:
    void check();
    void getCard();
    void checkOnline();
    uint8_t getQuality();

public:
    bool isOK();
    bool haveCard();
    bool isOnline();
    void startSocket();
    void startMqtt();
    void send();
    void setGnssOpen(bool open);
    bool readGnss();
    void init();
    bool isMqtt();
    bool isSocket();
    void sendSIM();
    void tick();
    void test();
    void sleep();
    
};

#endif