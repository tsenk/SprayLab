#include "doctest.h"

#include <windows.h>
#include <filesystem>
#include <fstream>

#include "persistence\store.h"
#include "logic\analysis.h"
#include "fixtures.h"

namespace fs = std::filesystem;

static std::wstring testDir() {
	wchar_t buf[MAX_PATH];
	GetEnvironmentVariableW(L"TEMP", buf, MAX_PATH);

	return std::wstring(buf)+L"\\sprayLabTests";
}

static void resetDir() {
	fs::remove_all(testDir());
	storeSetDir(testDir());
}

TEST_CASE("spray round-trips modulo ref") {
	resetDir();

	const WeaponRef* wref = weaponRef(Weapon::ak47);
	Spray sp = perfectSpray(*wref, 1.0f, 0.022f);
	grade(sp, *wref, 1.0f, 0.022f);
	Spray orig = sp;

	REQUIRE(storeSave(sp));
	CHECK(!sp.name.empty());

	REQUIRE(storeLoadAll());
	REQUIRE(storeSprays().size()==1);

	const Spray& back = storeSprays()[0];
	CHECK(back.name==sp.name);
	CHECK(back.weapon==orig.weapon);
	CHECK(back.dur==orig.dur);
	CHECK(back.epoch==orig.epoch);
	REQUIRE(back.bullets.size()==orig.bullets.size());
	REQUIRE(back.deltas.size()==orig.deltas.size());

	for (size_t i = 0; i<orig.bullets.size(); i++) {
		CHECK(back.bullets[i].actual.x==doctest::Approx(orig.bullets[i].actual.x));
		CHECK(back.bullets[i].actual.y==doctest::Approx(orig.bullets[i].actual.y));
		CHECK(back.deltas[i].delta.x==doctest::Approx(orig.deltas[i].delta.x));
	}
}

TEST_CASE("ref is absent on disk and rehydrates from the table") {
	resetDir();

	const WeaponRef* wref = weaponRef(Weapon::ak47);
	Spray sp = perfectSpray(*wref, 1.0f, 0.022f);
	grade(sp, *wref, 1.0f, 0.022f);
	REQUIRE(storeSave(sp));

	std::ifstream in(fs::path(testDir())/(sp.name+".spray"));
	std::string text((std::istreambuf_iterator<char>(in)), std::istreambuf_iterator<char>());
	CHECK(text.find("ref")==std::string::npos);

	REQUIRE(storeLoadAll());
	const Spray& back = storeSprays()[0];
	for (const Bullet& b : back.bullets) {
		CHECK(b.ref.x==wref->pattern[b.num].x);
		CHECK(b.ref.y==wref->pattern[b.num].y);
	}
}

TEST_CASE("name collisions append a number") {
	resetDir();

	const WeaponRef* wref = weaponRef(Weapon::ak47);
	Spray a = perfectSpray(*wref, 1.0f, 0.022f);
	Spray b = perfectSpray(*wref, 1.0f, 0.022f);
	a.epoch = 1000;
	b.epoch = 1000;

	REQUIRE(storeSave(a));
	REQUIRE(storeSave(b));
	CHECK(a.name!=b.name);
	CHECK(b.name==a.name+" 2");
}

TEST_CASE("delete removes the file and the entry") {
	resetDir();

	const WeaponRef* wref = weaponRef(Weapon::ak47);
	Spray sp = perfectSpray(*wref, 1.0f, 0.022f);
	REQUIRE(storeSave(sp));

	REQUIRE(storeDelete(sp.name));
	CHECK(!fs::exists(fs::path(testDir())/(sp.name+".spray")));

	REQUIRE(storeLoadAll());
	CHECK(storeSprays().empty());
}

TEST_CASE("load all skips corrupt files and sorts by epoch") {
	resetDir();

	const WeaponRef* wref = weaponRef(Weapon::ak47);
	Spray newer = perfectSpray(*wref, 1.0f, 0.022f);
	Spray older = perfectSpray(*wref, 1.0f, 0.022f);
	newer.epoch = 2000;
	older.epoch = 1000;

	REQUIRE(storeSave(newer));
	REQUIRE(storeSave(older));

	std::ofstream bad(fs::path(testDir())/"broken.spray");
	bad << "{ not json";
	bad.close();

	REQUIRE(storeLoadAll());
	REQUIRE(storeSprays().size()==2);
	CHECK(storeSprays()[0].epoch==1000);
	CHECK(storeSprays()[1].epoch==2000);
}
