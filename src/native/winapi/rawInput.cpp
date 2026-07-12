#include "rawInput.h"

#include <windows.h>

#include "capture.h"
#include "..\assets\weaponRef.h"
#include "..\abi\abi.h"

static HANDLE thread = nullptr;
static LARGE_INTEGER qpcFreq;
static LARGE_INTEGER qpcBase;
static Weapon wpn;
static int intv = 0;
static bool armed = false;
static bool reqRmb = false;
static bool rmbHeld = false;

static uint64_t nowMs() {
	LARGE_INTEGER now;
	QueryPerformanceCounter(&now);

	return uint64_t(now.QuadPart-qpcBase.QuadPart)*1000/uint64_t(qpcFreq.QuadPart);
}

static LRESULT CALLBACK wndProc(HWND wnd, UINT msg, WPARAM wp, LPARAM lp) {
	if (msg!=WM_INPUT)
		return DefWindowProcW(wnd, msg, wp, lp);

	RAWINPUT ri;
	UINT size = sizeof(ri);
	if (GetRawInputData(HRAWINPUT(lp), RID_INPUT, &ri, &size, sizeof(RAWINPUTHEADER))==UINT(-1) || ri.header.dwType!=RIM_TYPEMOUSE)
		return 0;

	const RAWMOUSE& m = ri.data.mouse;
	uint64_t t = nowMs();

	if (m.usButtonFlags&RI_MOUSE_RIGHT_BUTTON_DOWN)
		rmbHeld = true;
	if (m.usButtonFlags&RI_MOUSE_RIGHT_BUTTON_UP)
		rmbHeld = false;

	if (armed && (!reqRmb || rmbHeld) && (m.usButtonFlags&RI_MOUSE_LEFT_BUTTON_DOWN)) {
		// clicks inside our own gui are navigation, not sprays
		DWORD fgPid = 0;
		GetWindowThreadProcessId(GetForegroundWindow(), &fgPid);
		if (fgPid!=GetCurrentProcessId())
			captureBegin(wpn, intv, t);
	}

	if ((m.usFlags&MOUSE_MOVE_ABSOLUTE)==0 && (m.lLastX!=0 || m.lLastY!=0))
		captureMove(m.lLastX, m.lLastY, t);

	if (m.usButtonFlags&RI_MOUSE_LEFT_BUTTON_UP) {
		Spray sp;
		if (captureEnd(t, sp))
			abiCaptureDone(sp);
	}

	return 0;
}

static DWORD WINAPI captureLoop(void*) {
	WNDCLASSW wc = {};
	wc.lpfnWndProc = wndProc;
	wc.hInstance = GetModuleHandleW(nullptr);
	wc.lpszClassName = L"sprayLabCapture";
	RegisterClassW(&wc);

	HWND wnd = CreateWindowExW(0, wc.lpszClassName, L"", 0, 0, 0, 0, 0, HWND_MESSAGE, nullptr, wc.hInstance, nullptr);
	if (!wnd)
		return 1;

	// generic desktop page, mouse usage
	RAWINPUTDEVICE rid;
	rid.usUsagePage = 0x01;
	rid.usUsage = 0x02;
	rid.dwFlags = RIDEV_INPUTSINK;
	rid.hwndTarget = wnd;
	if (!RegisterRawInputDevices(&rid, 1, sizeof(rid))) {
		DestroyWindow(wnd);
		return 1;
	}

	QueryPerformanceFrequency(&qpcFreq);
	QueryPerformanceCounter(&qpcBase);

	MSG msg;
	while (GetMessageW(&msg, nullptr, 0, 0)>0) {
		TranslateMessage(&msg);
		DispatchMessageW(&msg);
	}

	return 0;
}

bool rawInputStart() {
	if (thread)
		return true;

	thread = CreateThread(nullptr, 0, captureLoop, nullptr, 0, nullptr);
	return thread!=nullptr;
}

void rawInputSetReqRmb(bool req) {
	reqRmb = req;
}

bool rawInputSetWeapon(Weapon w) {
	const WeaponRef* wref = weaponRef(w);
	armed = wref!=nullptr;
	if (!armed)
		return false;

	wpn = w;
	intv = wref->shotIntv;
	return true;
}
