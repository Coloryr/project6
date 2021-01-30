#ifndef TOOLS_h
#define TOOLS_h

#include "Arduino.h"

union Mytow
{
    uint8_t u8[2];
    uint16_t u16;
};

uint16_t makeuint16(int lsb, int msb);

#endif