using System.Text.Json;
using System.Text.Json.Serialization;

namespace SprayLab.Config;

public enum ClrScheme { Dark, Light }

public enum CaptureMode { Lmb, LmbRmb }

public class Cfg {
	public ClrScheme ClrScheme { get; set; } = ClrScheme.Dark;
	public CaptureMode CaptureMode { get; set; } = CaptureMode.Lmb;
	public float? Sens { get; set; }
	public float MYaw { get; set; } = 0.022f;
}

public static class CfgStore {
	public static Cfg Cur = new();

	static readonly string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SprayLab");
	static readonly string file = Path.Combine(dir, "config.json");

	static readonly JsonSerializerOptions opts = new() {
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		PropertyNameCaseInsensitive = true,
		WriteIndented = true,
		Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
	};

	public static void Load() {
		Directory.CreateDirectory(dir);

		try {
			Cur = JsonSerializer.Deserialize<Cfg>(File.ReadAllText(file), opts) ?? new Cfg();
		} catch {
			Cur = new Cfg();
		}

		if (Cur.Sens is < 0)
			Cur.Sens = null;
		if (Cur.MYaw<=0 || float.IsNaN(Cur.MYaw))
			Cur.MYaw = 0.022f;
	}

	public static void Save() {
		Directory.CreateDirectory(dir);

		File.WriteAllText(file, JsonSerializer.Serialize(Cur, opts));
	}
}
