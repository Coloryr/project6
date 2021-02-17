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
    bool ok;
    bool card;
    bool socket;
    bool online;
    bool mqtt;
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
    bool setGnssOpen(bool open);
    void readGnss();
    void startRead();
};

#endif