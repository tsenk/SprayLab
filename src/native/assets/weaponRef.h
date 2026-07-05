#pragma once

#include <vector>

#include "..\pos.h"
#include "..\weapon.h"

struct WeaponRef {
	int shotIntv; // ms
	std::vector<Pos> pattern; // degrees
};

const WeaponRef* weaponRef(Weapon wpn);
