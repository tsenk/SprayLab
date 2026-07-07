#include "analysis.h"

void grade(Spray& sp, const WeaponRef& wref, float sens, float mYaw) {
	int bulletC = int(sp.bullets.size());
	int patternC = int(wref.pattern.size());

	sp.deltas.clear();
	sp.deltas.reserve(bulletC);

	for (int i = 0; i<bulletC; i++) {
		Bullet& b = sp.bullets[i];
		b.actual.x = b.actual.x*sens*mYaw;
		b.actual.y = b.actual.y*sens*mYaw;

		// recoil is exhausted past the table, holds longer than the mag keep the last ref
		int n = i<patternC ? i : patternC-1;
		b.ref = wref.pattern[n];

		Delta d;
		d.num = b.num;
		d.delta.x = b.actual.x-b.ref.x;
		d.delta.y = b.actual.y-b.ref.y;

		sp.deltas.push_back(d);
	}
}
