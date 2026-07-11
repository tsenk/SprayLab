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

std::vector<Bullet> periodAvg(const std::vector<Spray>& sprays, Weapon wpn, int n, const WeaponRef& wref) {
	std::vector<const Spray*> recent;
	for (int i = int(sprays.size())-1; i>=0 && int(recent.size())<n; i--) {
		if (sprays[i].weapon==wpn)
			recent.push_back(&sprays[i]);
	}

	std::vector<Bullet> avg;
	if (recent.empty())
		return avg;

	int patternC = int(wref.pattern.size());
	avg.resize(patternC);

	for (int i = 0; i<patternC; i++) {
		float sx = 0;
		float sy = 0;
		int hit = 0;

		for (const Spray* sp : recent) {
			if (i>=int(sp->bullets.size()))
				continue;

			sx += sp->bullets[i].actual.x;
			sy += sp->bullets[i].actual.y;
			hit++;
		}

		avg[i].num = i;
		avg[i].ref = wref.pattern[i];
		if (hit>0) {
			avg[i].actual.x = sx/hit;
			avg[i].actual.y = sy/hit;
		}
	}

	return avg;
}
