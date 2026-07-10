#include "abi.h"

#include "..\winapi\rawInput.h"
#include "..\assets\weaponRef.h"
#include "..\logic\analysis.h"
#include "..\persistence\store.h"
#include "..\persistence\transfer.h"

#include <cstring>

static SprayCallback sprayCb = nullptr;
static float sens = 0;
static float mYaw = 0.022f;

static void emit(const Spray& sp) {
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

SL_API int slPing(void) {
	return 1337;
}

SL_API void slSetSprayCallback(SprayCallback cb) {
	sprayCb = cb;
}

SL_API int slInit(void) {
	if (!storeLoadAll())
		return 0;

	for (const Spray& sp : storeSprays())
		emit(sp);

	return int(storeSprays().size());
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

SL_API void slSetCaptureMode(int reqRmb) {
	rawInputSetReqRmb(reqRmb!=0);
}

SL_API int slDeleteSpray(const char* name) {
	return storeDelete(name) ? 1 : 0;
}

SL_API int slRenameSpray(const char* oldName, const char* newName, char* outFinal, int cap) {
	std::string final;
	if (!storeRename(oldName, newName, final))
		return 0;

	if (int(final.size())+1>cap)
		return 0;

	memcpy(outFinal, final.c_str(), final.size()+1);
	return 1;
}

SL_API int slImportSpray(const wchar_t* path) {
	if (!transferImport(path))
		return 0;

	emit(storeSprays().back());
	return 1;
}

// fires on the capture thread, the callback must copy before returning
void abiCaptureDone(Spray& sp) {
	const WeaponRef* wref = weaponRef(sp.weapon);
	if (wref)
		grade(sp, *wref, sens, mYaw);

	storeSave(sp);
	emit(sp);
}
