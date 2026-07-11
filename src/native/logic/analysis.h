#pragma once

#include "spray.h"
#include "..\assets\weaponRef.h"

void grade(Spray& sp, const WeaponRef& wref, float sens, float mYaw);
std::vector<Bullet> periodAvg(const std::vector<Spray>& sprays, Weapon wpn, int n, const WeaponRef& wref);
