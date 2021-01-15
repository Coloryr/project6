#include "log.h"

void log_info(char *data)
{
    printf("[Info]%s\n", data);
}

void log_error(char *data)
{
    printf("[Error]%s\n", data);
}

void log_info_u8(char *data, uint8_t data1)
{
    printf("[Info]%s%d\n", data, data1);
}

void log_info_u16(char *data, uint16_t data1)
{
    printf("[Info]%s%d\n", data, data1);
}