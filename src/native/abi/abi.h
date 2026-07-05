#pragma once

#define SL_API extern "C" __declspec(dllexport)

SL_API int slPing(void);
