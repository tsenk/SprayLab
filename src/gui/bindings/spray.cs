namespace SprayLab.Bindings;

public enum Weapon {
	Ak47, M4a4, M4a1s, Galil, Famas, Sg553, Aug,
	Mp9, Mac10, Mp7, Mp5sd, Ump45, P90, Bizon, Cz75
}

public struct Pos {
	public float X;
	public float Y;
}

public struct Bullet {
	public int Num;
	public Pos Actual;
	public Pos Ref;
}

public struct Delta {
	public int Num;
	public Pos Value;
}

public class Spray {
	public string Name = "";
	public Weapon Weapon;
	public ulong Dur;
	public long Epoch;
	public List<Bullet> Bullets = new();
	public List<Delta> Deltas = new();
}
