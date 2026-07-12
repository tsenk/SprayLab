using Microsoft.UI.Xaml.Controls;
using SprayLab.Bindings;

namespace SprayLab.Widgets;

public class MistakeRow {
	public string Num { get; init; } = "";
	public string Dev { get; init; } = "";
}

public sealed partial class MistakeList : UserControl {
	public MistakeList() {
		InitializeComponent();
	}

	public void Show(IReadOnlyList<Delta> flagged) {
		var list = new List<MistakeRow>();

		for (int i = 0; i<flagged.Count; i++)
			list.Add(new MistakeRow {
				Num = (i+1).ToString(),
				Dev = $"X: {flagged[i].Value.X:0.00} Y: {flagged[i].Value.Y:0.00}",
			});

		// a clean spray should not look like a broken list
		if (list.Count==0)
			list.Add(new MistakeRow { Num = "", Dev = "None above threshold" });

		rows.ItemsSource = list;
	}
}
