#pragma once

#include <vector>
#include <cstdint>

#include "logic\spray.h"
#include "assets\weaponRef.h"

struct MoveEvent {
	uint64_t t;
	int dx;
	int dy;
};

// two moves per 100ms window, a move exactly on a boundary lands in the next bullet
const std::vector<MoveEvent> SYNTH_STREAM = {
	{10, 1, 2}, {60, 2, 3},
	{110, 4, 5}, {160, 1, 1},
	{210, 2, 2},
};

// actual in counts that grade back onto the ref exactly, zero deltas, no drift
inline Spray perfectSpray(const WeaponRef& wref, float sens, float mYaw) {
	Spray sp;
	sp.weapon = Weapon::ak47;
	sp.dur = uint64_t(wref.shotIntv)*(wref.pattern.size()-1);

	for (int i = 0; i<int(wref.pattern.size()); i++) {
		Bullet b;
		b.num = i;
		b.actual = {wref.pattern[i].x/(sens*mYaw), wref.pattern[i].y/(sens*mYaw)};
		b.ref = {0, 0};

		sp.bullets.push_back(b);
	}

	return sp;
}
