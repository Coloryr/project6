#ifndef UPLOAD_h
#define UPLOAD_h

#include "Arduino.h"

class Upload
{
private:
    uint8_t *readbuff;
    uint8_t *writebuff;
    bool open;
    void check();
    void sendRead(uint8_t type);
    void sendWrite(uint8_t size);
    void sendOpen();
    void buildPack(uint8_t type);
    void sendOK();
    void send(uint8_t size);
    void reset();

public:
    Upload();
    void tick();
};

#endif