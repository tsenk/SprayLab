#include "doctest.h"

#include <cmath>

#include "winapi\capture.h"
#include "logic\analysis.h"
#include "fixtures.h"

TEST_CASE("grade converts counts to degrees with sens times myaw") {
	Spray sp;
	sp.weapon = Weapon::ak47;
	sp.bullets = { {0, {0, 0}, {0, 0}}, {1, {100, 200}, {0, 0}} };

	const WeaponRef* wref = weaponRef(Weapon::ak47);
	grade(sp, *wref, 2.0f, 0.022f);

	CHECK(sp.bullets[1].actual.x==doctest::Approx(100*2.0f*0.022f));
	CHECK(sp.bullets[1].actual.y==doctest::Approx(200*2.0f*0.022f));
}

TEST_CASE("grade fills ref from the table and caches deltas") {
	Spray sp;
	sp.weapon = Weapon::ak47;
	sp.bullets = { {0, {0, 0}, {0, 0}}, {1, {0, 0}, {0, 0}} };

	const WeaponRef* wref = weaponRef(Weapon::ak47);
	grade(sp, *wref, 1.0f, 0.022f);

	CHECK(sp.bullets[0].ref.x==wref->pattern[0].x);
	CHECK(sp.bullets[1].ref.y==wref->pattern[1].y);

	REQUIRE(sp.deltas.size()==2);
	CHECK(sp.deltas[1].num==1);
	CHECK(sp.deltas[1].delta.x==doctest::Approx(-wref->pattern[1].x));
	CHECK(sp.deltas[1].delta.y==doctest::Approx(-wref->pattern[1].y));
}

TEST_CASE("perfect spray grades to zero deltas") {
	const WeaponRef* wref = weaponRef(Weapon::ak47);
	Spray sp = perfectSpray(*wref, 1.5f, 0.022f);

	grade(sp, *wref, 1.5f, 0.022f);

	for (const Delta& d : sp.deltas) {
		CHECK(d.delta.x==doctest::Approx(0.0f).epsilon(0.001));
		CHECK(d.delta.y==doctest::Approx(0.0f).epsilon(0.001));
	}
}

TEST_CASE("holds past the pattern clamp ref to the last entry") {
	const WeaponRef* wref = weaponRef(Weapon::ak47);
	int patternC = int(wref->pattern.size());

	Spray sp;
	sp.weapon = Weapon::ak47;
	for (int i = 0; i<patternC+3; i++)
		sp.bullets.push_back({i, {0, 0}, {0, 0}});

	grade(sp, *wref, 1.0f, 0.022f);

	CHECK(sp.bullets[patternC+2].ref.x==wref->pattern[patternC-1].x);
	CHECK(sp.bullets[patternC+2].ref.y==wref->pattern[patternC-1].y);
}

TEST_CASE("synthetic stream flows capture to grade as one spine") {
	captureBegin(Weapon::ak47, 100, 0);
	for (const MoveEvent& e : SYNTH_STREAM)
		captureMove(e.dx, e.dy, e.t);

	Spray sp;
	REQUIRE(captureEnd(260, sp));

	const WeaponRef* wref = weaponRef(Weapon::ak47);
	grade(sp, *wref, 1.0f, 0.022f);

	CHECK(sp.bullets[2].actual.x==doctest::Approx(8*0.022f));
	CHECK(sp.bullets[2].actual.y==doctest::Approx(11*0.022f));
	CHECK(sp.deltas[2].delta.x==doctest::Approx(8*0.022f-wref->pattern[2].x));
	CHECK(sp.deltas[2].delta.y==doctest::Approx(11*0.022f-wref->pattern[2].y));
}
