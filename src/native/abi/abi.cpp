#include "abi.h"

#include "..\winapi\rawInput.h"
#include "..\assets\weaponRef.h"

static SprayCallback sprayCb = nullptr;

SL_API int slPing(void) {
	return 1337;
}

SL_API void slSetSprayCallback(SprayCallback cb) {
	sprayCb = cb;
}

SL_API int slRegisterCapture(void) {
	return rawInputStart() ? 1 : 0;
}

SL_API int slSetWeapon(int weapon) {
	return rawInputSetWeapon(Weapon(weapon)) ? 1 : 0;
}

SL_API int slWeaponRef(int weapon, int* shotIntv, Pos* pattern, int cap) {
	const WeaponRef* wref = weaponRef(Weapon(weapon));
	if (!wref)
		return 0;

	*shotIntv = wref->shotIntv;

	int n = int(wref->pattern.size());
	if (n>cap)
		n = cap;

	for (int i = 0; i<n; i++)
		pattern[i] = wref->pattern[i];

	return n;
}

// fires on the capture thread, the callback must copy before returning
void abiEmitSpray(const Spray& sp) {
	if (!sprayCb)
		return;

	SlSpray flat;
	flat.name = sp.name.c_str();
	flat.weapon = int(sp.weapon);
	flat.dur = sp.dur;
	flat.epoch = int64_t(sp.epoch);
	flat.bullets = sp.bullets.data();
	flat.bulletC = int(sp.bullets.size());
	flat.deltas = sp.deltas.data();
	flat.deltaC = int(sp.deltas.size());

	sprayCb(&flat);
}
