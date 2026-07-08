#include "spray.h"

#include "..\vendor\json.hpp"

using nlohmann::json;

// enum identifier survives reordering, display name feeds default filenames
static const char* WEAPON_IDS[] = {
	"ak47", "m4a4", "m4a1s", "galil", "famas", "sg553", "aug",
	"mp9", "mac10", "mp7", "mp5sd", "ump45", "p90", "bizon", "cz75",
};

static const char* WEAPON_DISPLAY[] = {
	"AK-47", "M4A4", "M4A1-S", "Galil AR", "FAMAS", "SG 553", "AUG",
	"MP9", "MAC-10", "MP7", "MP5-SD", "UMP-45", "P90", "PP-Bizon", "CZ75-Auto",
};

const char* weaponId(Weapon wpn) {
	return WEAPON_IDS[int(wpn)];
}

const char* weaponDisplay(Weapon wpn) {
	return WEAPON_DISPLAY[int(wpn)];
}

bool weaponFromId(const std::string& id, Weapon& out) {
	for (int i = 0; i<int(std::size(WEAPON_IDS)); i++) {
		if (id==WEAPON_IDS[i]) {
			out = Weapon(i);
			return true;
		}
	}

	return false;
}

std::string sprayToJson(const Spray& sp) {
	json j;
	j["weapon"] = weaponId(sp.weapon);
	j["dur"] = sp.dur;
	j["epoch"] = int64_t(sp.epoch);

	json bullets = json::array();
	for (const Bullet& b : sp.bullets)
		bullets.push_back({ {"num", b.num}, {"x", b.actual.x}, {"y", b.actual.y} });
	j["bullets"] = bullets;

	json deltas = json::array();
	for (const Delta& d : sp.deltas)
		deltas.push_back({ {"num", d.num}, {"x", d.delta.x}, {"y", d.delta.y} });
	j["deltas"] = deltas;

	return j.dump();
}

bool sprayFromJson(const std::string& text, Spray& out) {
	json j = json::parse(text, nullptr, false);
	if (j.is_discarded())
		return false;

	if (!j.contains("weapon") || !j.contains("dur") || !j.contains("epoch") || !j.contains("bullets"))
		return false;

	if (!weaponFromId(j["weapon"].get<std::string>(), out.weapon))
		return false;

	out.dur = j["dur"].get<uint64_t>();
	out.epoch = std::time_t(j["epoch"].get<int64_t>());

	out.bullets.clear();
	for (const json& e : j["bullets"]) {
		Bullet b;
		b.num = e["num"].get<int>();
		b.actual = {e["x"].get<float>(), e["y"].get<float>()};
		b.ref = {0, 0};

		out.bullets.push_back(b);
	}

	out.deltas.clear();
	if (j.contains("deltas")) {
		for (const json& e : j["deltas"]) {
			Delta d;
			d.num = e["num"].get<int>();
			d.delta = {e["x"].get<float>(), e["y"].get<float>()};

			out.deltas.push_back(d);
		}
	}

	return true;
}
