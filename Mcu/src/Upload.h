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
    void sendread(uint8_t type);
    void sendwrite(uint8_t size);
    void sendopen();
    void buildpack(uint8_t type);
    void sendok();
    void send(uint8_t size);
    void reset();

public:
    Upload();
    void tick();
};

#endif