#ifndef NBIOT_h
#define NBIOT_h

#include "Arduino.h"

extern uint8_t UUID[16];

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
    void startMqtt(uint8_t *User, uint8_t *Pass);
    void send(uint8_t *data);
    bool setGnssOpen(bool open);
    void readGnss();

};

#endif