using System.Runtime.InteropServices;

namespace SprayLab.Bindings;

internal static class Abi {
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void SprayCallback(IntPtr spray);

	[StructLayout(LayoutKind.Sequential)]
	internal struct SlBullet {
		public int Num;
		public Pos Actual;
		public Pos Ref;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct SlSpray {
		public IntPtr Name;
		public int Weapon;
		public ulong Dur;
		public long Epoch;
		public IntPtr Bullets;
		public int BulletC;
		public IntPtr Deltas;
		public int DeltaC;
	}

	[DllImport("slnative", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int slPing();

	[DllImport("slnative", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void slSetSprayCallback(SprayCallback cb);

	[DllImport("slnative", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int slInit();

	[DllImport("slnative", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int slRegisterCapture();

	[DllImport("slnative", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void slSetCaptureMode(int reqRmb);

	[DllImport("slnative", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int slDeleteSpray([MarshalAs(UnmanagedType.LPStr)] string name);

	[DllImport("slnative", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	internal static extern int slRenameSpray(string oldName, string newName, System.Text.StringBuilder outFinal, int cap);

	[DllImport("slnative", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int slImportSpray([MarshalAs(UnmanagedType.LPWStr)] string path);

	[DllImport("slnative", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int slPeriodAvg(int weapon, int n, [Out] SlBullet[] outAvg, int cap);

	[DllImport("slnative", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int slWeaponRef(int weapon, ref int shotIntv, [Out] Pos[] pattern, int cap);

	static readonly Dictionary<Weapon, Pos[]> patterns = new();

	// full pattern backs every graph so the scale never depends on spray length
	internal static Pos[] WeaponPattern(Weapon weapon) {
		lock (patterns) {
			if (patterns.TryGetValue(weapon, out var known))
				return known;

			int intv = 0;
			var buf = new Pos[128];
			int n = slWeaponRef((int)weapon, ref intv, buf, buf.Length);

			var pattern = buf[..n];
			patterns[weapon] = pattern;
			return pattern;
		}
	}

	[DllImport("slnative", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int slSetWeapon(int weapon);

	[DllImport("slnative", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void slSetConversion(float sens, float mYaw);

	internal static Spray ReadSpray(IntPtr p) {
		var flat = Marshal.PtrToStructure<SlSpray>(p);

		var sp = new Spray {
			Name = Marshal.PtrToStringAnsi(flat.Name) ?? "",
			Weapon = (Weapon)flat.Weapon,
			Dur = flat.Dur,
			Epoch = flat.Epoch,
		};

		int bulletSize = Marshal.SizeOf<Bullet>();
		for (int i = 0; i<flat.BulletC; i++)
			sp.Bullets.Add(Marshal.PtrToStructure<Bullet>(IntPtr.Add(flat.Bullets, i*bulletSize))!);

		int deltaSize = Marshal.SizeOf<Delta>();
		for (int i = 0; i<flat.DeltaC; i++)
			sp.Deltas.Add(Marshal.PtrToStructure<Delta>(IntPtr.Add(flat.Deltas, i*deltaSize)));

		return sp;
	}
}
