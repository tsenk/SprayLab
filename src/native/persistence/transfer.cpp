#include "transfer.h"

#include <filesystem>
#include <fstream>

#include "store.h"
#include "..\assets\weaponRef.h"

namespace fs = std::filesystem;

bool transferImport(const std::wstring& path) {
	std::ifstream in(fs::path{path});
	if (!in)
		return false;

	std::string text((std::istreambuf_iterator<char>(in)), std::istreambuf_iterator<char>());

	Spray sp;
	if (!sprayFromJson(text, sp))
		return false;

	sp.name = fs::path(path).stem().string();

	const WeaponRef* wref = weaponRef(sp.weapon);
	if (wref) {
		int patternC = int(wref->pattern.size());
		for (Bullet& b : sp.bullets)
			b.ref = wref->pattern[b.num<patternC ? b.num : patternC-1];
	}

	return storeSave(sp);
}
