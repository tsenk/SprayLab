#pragma once

#include <cstdint>

#include "..\weapon.h"
#include "..\logic\spray.h"

void captureBegin(Weapon wpn, int shotIntv, uint64_t t);
void captureMove(int dx, int dy, uint64_t t);
bool captureEnd(uint64_t t, Spray& out);
