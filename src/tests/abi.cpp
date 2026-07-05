#define DOCTEST_CONFIG_IMPLEMENT_WITH_MAIN
#include "doctest.h"

#include <windows.h>

TEST_CASE("harness smoke") {
	CHECK(1+1==2);
}

TEST_CASE("slPing round-trips across the dll boundary") {
	HMODULE dll = LoadLibraryW(L"slnative.dll");
	REQUIRE(dll);

	auto ping = reinterpret_cast<int(*)()>(GetProcAddress(dll, "slPing"));
	REQUIRE(ping);
	CHECK(ping()==1337);

	FreeLibrary(dll);
}
