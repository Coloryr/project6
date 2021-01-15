#ifndef LOG_h
#define LOG_h

#include "Arduino.h"

void log_info(char *data);
void log_error(char *data);
void log_info_u8(char *data, uint8_t data1);
void log_info_u16(char *data, uint16_t data1);

#endif