#pragma once

#include <vector>
#include <cstdint>

struct MoveEvent {
	uint64_t t;
	int dx;
	int dy;
};

// two moves per 100ms window, a move exactly on a boundary lands in the next bullet
const std::vector<MoveEvent> SYNTH_STREAM = {
	{10, 1, 2}, {60, 2, 3},
	{110, 4, 5}, {160, 1, 1},
	{210, 2, 2},
};
