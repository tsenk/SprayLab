using System.Runtime.InteropServices;

namespace SprayLab.Bindings;

internal static class Abi {
	[DllImport("slnative", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int slPing();
}
