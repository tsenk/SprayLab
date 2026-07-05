#include "capture.h"

static Spray sp;
static Pos running;
static uint64_t anchor;
static uint64_t nextBoundary;
static int intv;
static bool active = false;

static void snapshotDue(uint64_t rel) {
	while (nextBoundary<=rel) {
		Bullet b;
		b.num = int(sp.bullets.size());
		b.actual = running;
		b.ref = {0, 0};

		sp.bullets.push_back(b);
		nextBoundary += intv;
	}
}

void captureBegin(Weapon wpn, int shotIntv, uint64_t t) {
	sp = {};
	sp.weapon = wpn;
	sp.epoch = std::time(nullptr);

	running = {0, 0};
	anchor = t;
	intv = shotIntv;
	nextBoundary = intv;
	active = true;

	Bullet b;
	b.num = 0;
	b.actual = running;
	b.ref = {0, 0};

	sp.bullets.push_back(b);
}

void captureMove(int dx, int dy, uint64_t t) {
	if (!active)
		return;

	snapshotDue(t-anchor);

	running.x += float(dx);
	running.y += float(dy);
}

bool captureEnd(uint64_t t, Spray& out) {
	if (!active)
		return false;

	active = false;

	uint64_t dur = t-anchor;
	if (dur<uint64_t(intv))
		return false;

	snapshotDue(dur);
	sp.dur = dur;

	out = std::move(sp);
	return true;
}
