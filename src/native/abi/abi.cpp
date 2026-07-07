#include "abi.h"

#include "..\winapi\rawInput.h"
#include "..\assets\weaponRef.h"
#include "..\logic\analysis.h"

static SprayCallback sprayCb = nullptr;
static float sens = 0;
static float mYaw = 0.022f;

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

SL_API void slSetConversion(float s, float y) {
	sens = s;
	mYaw = y;
}

// fires on the capture thread, the callback must copy before returning
void abiEmitSpray(Spray& sp) {
	if (!sprayCb)
		return;

	const WeaponRef* wref = weaponRef(sp.weapon);
	if (wref)
		grade(sp, *wref, sens, mYaw);

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
