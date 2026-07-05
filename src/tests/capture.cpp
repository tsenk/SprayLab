#include "doctest.h"

#include "winapi\capture.h"
#include "fixtures.h"

TEST_CASE("bullet count derives from duration") {
	captureBegin(Weapon::ak47, 100, 0);

	Spray sp;
	REQUIRE(captureEnd(990, sp));
	CHECK(sp.bullets.size()==10);
	CHECK(sp.dur==990);
	CHECK(sp.weapon==Weapon::ak47);
}

TEST_CASE("boundary landing exactly on release still fires") {
	captureBegin(Weapon::ak47, 100, 0);

	Spray sp;
	REQUIRE(captureEnd(200, sp));
	CHECK(sp.bullets.size()==3);
}

TEST_CASE("first shot snapshots at t=0 before any movement") {
	captureBegin(Weapon::ak47, 100, 1000);
	captureMove(3, 7, 1050);

	Spray sp;
	REQUIRE(captureEnd(1200, sp));
	CHECK(sp.bullets[0].num==0);
	CHECK(sp.bullets[0].actual.x==0);
	CHECK(sp.bullets[0].actual.y==0);
}

TEST_CASE("sub 2 bullet holds discard") {
	captureBegin(Weapon::ak47, 100, 0);
	captureMove(5, 5, 40);

	Spray sp;
	CHECK(!captureEnd(99, sp));
}

TEST_CASE("end without begin discards") {
	Spray sp;
	CHECK(!captureEnd(500, sp));
}

TEST_CASE("snapshots accumulate the synthetic stream per boundary") {
	captureBegin(Weapon::ak47, 100, 0);
	for (const MoveEvent& e : SYNTH_STREAM)
		captureMove(e.dx, e.dy, e.t);

	Spray sp;
	REQUIRE(captureEnd(260, sp));
	REQUIRE(sp.bullets.size()==3);

	CHECK(sp.bullets[0].actual.x==0);
	CHECK(sp.bullets[0].actual.y==0);

	CHECK(sp.bullets[1].actual.x==3);
	CHECK(sp.bullets[1].actual.y==5);

	CHECK(sp.bullets[2].actual.x==8);
	CHECK(sp.bullets[2].actual.y==11);

	for (int i = 0; i<3; i++)
		CHECK(sp.bullets[i].num==i);
}
