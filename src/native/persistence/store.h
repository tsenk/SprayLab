#pragma once

#include <string>
#include <vector>

#include "..\logic\spray.h"

void storeSetDir(const std::wstring& dir);
bool storeLoadAll();
std::vector<Spray>& storeSprays();
bool storeSave(Spray& sp);
bool storeDelete(const std::string& name);
