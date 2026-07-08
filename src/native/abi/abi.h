#pragma once

#include <cstdint>

#include "..\pos.h"
#include "..\logic\spray.h"

#define SL_API extern "C" __declspec(dllexport)

struct SlSpray {
	const char* name;
	int weapon;
	uint64_t dur;
	int64_t epoch;
	const Bullet* bullets;
	int bulletC;
	const Delta* deltas;
	int deltaC;
};

typedef void(__cdecl* SprayCallback)(const SlSpray* sp);

SL_API int slPing(void);
SL_API void slSetSprayCallback(SprayCallback cb);
SL_API int slInit(void);
SL_API int slRegisterCapture(void);
SL_API int slSetWeapon(int weapon);
SL_API void slSetConversion(float sens, float mYaw);
SL_API void slSetCaptureMode(int reqRmb);
SL_API int slDeleteSpray(const char* name);

void abiCaptureDone(Spray& sp);
