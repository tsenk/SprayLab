using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SprayLab.Bindings;
using SprayLab.Viewer;

namespace SprayLab.Main;

public sealed partial class MainView : UserControl {
	static readonly string[] WEAPON_NAMES = {
		"AK-47", "M4A4", "M4A1-S", "Galil AR", "FAMAS", "SG 553", "AUG",
		"MP9", "MAC-10", "MP7", "MP5-SD", "UMP-45", "P90", "PP-Bizon", "CZ75-Auto",
	};

	readonly MainViewModel vm = new();

	public MainView() {
		InitializeComponent();

		cmbCapture.ItemsSource = new[] { "LMB", "LMB+RMB (Combined)" };
		cmbCapture.SelectedIndex = 0;
		cmbWeapon.ItemsSource = WEAPON_NAMES;
		cmbWeapon.SelectedIndex = 0;
		numMYaw.Value = 0.022;
		numThresh.Value = vm.Thresh;
		numSens.Value = double.NaN;

		vm.PropertyChanged += (s, e) => onSprayChanged();

		arm();
	}

	public void ShowSpray(Spray sp) {
		vm.LastSpray = sp;
	}

	void onSprayChanged() {
		Spray? sp = vm.LastSpray;
		if (sp==null)
			return;

		txtDur.Text = $"Duration: {sp.Dur} ms";
		txtBullets.Text = $"Bullets Fired: {sp.Bullets.Count}";

		applyThreshold();
	}

	void applyThreshold() {
		if (vm.LastSpray==null)
			return;

		var flagged = Threshold.Flag(vm.LastSpray.Deltas, vm.Thresh);

		mistakes.Show(flagged);
		graph.Show(vm.LastSpray.Bullets, flagged);
	}

	void arm() {
		bool valid = !float.IsNaN(vm.Sens) && vm.Sens>=0 && vm.MYaw>0;

		int armed = 0;
		if (valid) {
			Abi.slSetConversion(vm.Sens, vm.MYaw);
			armed = Abi.slSetWeapon((int)vm.SelectedWeapon);
		} else
			Abi.slSetWeapon(-1);

		txtWarning.Visibility = armed==1 ? Visibility.Collapsed : Visibility.Visible;
	}

	void onApply(object sender, RoutedEventArgs e) {
		vm.Thresh = double.IsNaN(numThresh.Value) ? 0 : (float)numThresh.Value;

		applyThreshold();
	}

	void onSensChanged(NumberBox sender, NumberBoxValueChangedEventArgs e) {
		vm.Sens = (float)sender.Value;

		arm();
	}

	void onMYawChanged(NumberBox sender, NumberBoxValueChangedEventArgs e) {
		vm.MYaw = (float)sender.Value;

		arm();
	}

	void onWeaponChanged(object sender, SelectionChangedEventArgs e) {
		vm.SelectedWeapon = (Weapon)cmbWeapon.SelectedIndex;

		arm();
	}

	void onCaptureChanged(object sender, SelectionChangedEventArgs e) {
		vm.CaptureMode = cmbCapture.SelectedIndex;
	}
}
