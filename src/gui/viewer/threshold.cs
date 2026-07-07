using SprayLab.Bindings;

namespace SprayLab.Viewer;

public static class Threshold {
	// 360 directional magnitude in degrees, drift origin is the first flagged bullet
	public static List<Delta> Flag(IEnumerable<Delta> deltas, float thresh) {
		var flagged = new List<Delta>();

		foreach (Delta d in deltas) {
			float mag = MathF.Sqrt(d.Value.X*d.Value.X+d.Value.Y*d.Value.Y);
			if (mag>thresh)
				flagged.Add(d);
		}

		return flagged;
	}
}
