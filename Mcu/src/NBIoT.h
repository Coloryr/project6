#ifndef NBIOT_h
#define NBIOT_h

#include "Arduino.h"

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
    NBIoT();
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