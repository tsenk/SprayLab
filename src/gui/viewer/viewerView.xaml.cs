using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SprayLab.Bindings;
using Windows.System;

namespace SprayLab.Viewer;

public sealed partial class ViewerView : UserControl {
	Spray? current;
	float thresh = 1.0f;

	public Spray? Current => current;

	public event Action? BackRequested;
	public event Action<Spray>? DeleteRequested;
	public event Action<Spray, string>? RenameRequested;
	public event Action<int>? StepRequested;

	public ViewerView() {
		InitializeComponent();

		numThresh.Value = thresh;
	}

	public void Show(Spray sp) {
		current = sp;
		txtName.Text = sp.Name;
		txtDur.Text = $"Duration: {sp.Dur} ms";
		txtBullets.Text = $"Bullets Fired: {sp.Bullets.Count}";

		applyThreshold();
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
