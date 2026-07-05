#pragma once

#include <string>
#include <vector>
#include <ctime>
#include <cstdint>

#include "..\weapon.h"
#include "bullet.h"
#include "delta.h"

struct Spray {
	std::string name;
	Weapon weapon;
	uint64_t dur; // ms
	std::time_t epoch;
	std::vector<Bullet> bullets;
	std::vector<Delta> deltas;
};
