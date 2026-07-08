#include "store.h"

#include <windows.h>
#include <filesystem>
#include <fstream>
#include <algorithm>

#include "..\assets\weaponRef.h"

namespace fs = std::filesystem;

static std::wstring dir;
static std::vector<Spray> sprays;

static fs::path sprayPath(const std::string& name) {
	return fs::path(dir)/fs::path(name+".spray");
}

static void ensureDir() {
	if (dir.empty()) {
		wchar_t buf[MAX_PATH];
		GetEnvironmentVariableW(L"APPDATA", buf, MAX_PATH);
		dir = std::wstring(buf)+L"\\SprayLab";
	}

	CreateDirectoryW(dir.c_str(), nullptr);
}

void storeSetDir(const std::wstring& d) {
	dir = d;
	CreateDirectoryW(dir.c_str(), nullptr);
}

std::vector<Spray>& storeSprays() {
	return sprays;
}

bool storeLoadAll() {
	ensureDir();
	sprays.clear();

	for (const fs::directory_entry& e : fs::directory_iterator(dir)) {
		if (e.path().extension()!=".spray")
			continue;

		std::ifstream in(e.path());
		std::string text((std::istreambuf_iterator<char>(in)), std::istreambuf_iterator<char>());

		Spray sp;
		if (!sprayFromJson(text, sp))
			continue;

		sp.name = e.path().stem().string();

		const WeaponRef* wref = weaponRef(sp.weapon);
		if (wref) {
			int patternC = int(wref->pattern.size());
			for (Bullet& b : sp.bullets)
				b.ref = wref->pattern[b.num<patternC ? b.num : patternC-1];
		}

		sprays.push_back(std::move(sp));
	}

	std::sort(sprays.begin(), sprays.end(), [](const Spray& a, const Spray& b) { return a.epoch<b.epoch; });
	return true;
}

bool storeSave(Spray& sp) {
	ensureDir();

	if (sp.name.empty()) {
		tm t;
		localtime_s(&t, &sp.epoch);

		char buf[96];
		snprintf(buf, sizeof(buf), "%s %02d-%02d-%02d %02d-%02d-%02d", weaponDisplay(sp.weapon), t.tm_year%100, t.tm_mon+1, t.tm_mday, t.tm_hour, t.tm_min, t.tm_sec);
		sp.name = buf;
	}

	std::string base = sp.name;
	for (int n = 2; fs::exists(sprayPath(sp.name)); n++)
		sp.name = base+" "+std::to_string(n);

	std::ofstream out(sprayPath(sp.name));
	if (!out)
		return false;

	out << sprayToJson(sp);
	sprays.push_back(sp);
	return true;
}

bool storeDelete(const std::string& name) {
	std::error_code ec;
	if (!fs::remove(sprayPath(name), ec))
		return false;

	for (size_t i = 0; i<sprays.size(); i++) {
		if (sprays[i].name==name) {
			sprays.erase(sprays.begin()+i);
			break;
		}
	}

	return true;
}
