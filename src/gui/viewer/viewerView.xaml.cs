using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SprayLab.Bindings;
using Windows.System;

namespace SprayLab.Viewer;

public sealed partial class ViewerView : UserControl {
	Spray? current;
	float thresh = 1.0f;

	// comparison window, view state, resets per app run
	int recentC = 10;

	public Spray? Current => current;

	public event Action? BackRequested;
	public event Action<Spray>? DeleteRequested;
	public event Action<Spray, string>? RenameRequested;
	public event Action<int>? StepRequested;

	public ViewerView() {
		InitializeComponent();

		numThresh.Value = thresh;
		numRecent.Value = recentC;
	}

	public void Show(Spray sp) {
		current = sp;
		txtName.Text = sp.Name;
		txtDur.Text = $"Duration: {sp.Dur} ms";
		txtBullets.Text = $"Bullets Fired: {sp.Bullets.Count}";

		applyThreshold();
		refreshAvg();
	}

	void refreshAvg() {
		if (current==null)
			return;

		var buf = new Abi.SlBullet[64];
		int c = Abi.slPeriodAvg((int)current.Weapon, recentC, buf, buf.Length);

		var bullets = new List<Bullet>();
		for (int i = 0; i<c; i++)
			bullets.Add(new Bullet { Num = buf[i].Num, Actual = buf[i].Actual, Ref = buf[i].Ref });

		avgGraph.Show(bullets, Array.Empty<Delta>());
	}

	void onRecentChanged(NumberBox sender, NumberBoxValueChangedEventArgs e) {
		if (double.IsNaN(sender.Value) || sender.Value<1)
			return;

		recentC = (int)sender.Value;

		refreshAvg();
	}

	public void ApplyName(string name) {
		txtName.Text = name;
	}

	void applyThreshold() {
		if (current==null)
			return;

		var flagged = Threshold.Flag(current.Deltas, thresh);

		mistakes.Show(flagged);
		graph.Show(current.Bullets, flagged);
	}

	void onApply(object sender, RoutedEventArgs e) {
		thresh = double.IsNaN(numThresh.Value) ? 0 : (float)numThresh.Value;

		applyThreshold();
	}

	void onBack(object sender, RoutedEventArgs e) {
		BackRequested?.Invoke();
	}

	void onDelete(object sender, RoutedEventArgs e) {
		if (current!=null)
			DeleteRequested?.Invoke(current);
	}

	void onPrev(object sender, RoutedEventArgs e) {
		StepRequested?.Invoke(-1);
	}

	void onNext(object sender, RoutedEventArgs e) {
		StepRequested?.Invoke(1);
	}

	void onNameKey(object sender, KeyRoutedEventArgs e) {
		if (e.Key==VirtualKey.Enter)
			commitName();
	}

	void onNameCommit(object sender, RoutedEventArgs e) {
		commitName();
	}

	void commitName() {
		if (current==null)
			return;

		string wanted = txtName.Text.Trim();

		if (wanted.Length==0 || wanted==current.Name) {
			txtName.Text = current.Name;
			return;
		}

		RenameRequested?.Invoke(current, wanted);
	}
}
