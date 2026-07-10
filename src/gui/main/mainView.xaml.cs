using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SprayLab.Bindings;
using SprayLab.Config;
using SprayLab.Viewer;

namespace SprayLab.Main;

public sealed partial class MainView : UserControl {
	static readonly string[] WEAPON_NAMES = {
		"AK-47", "M4A4", "M4A1-S", "Galil AR", "FAMAS", "SG 553", "AUG",
		"MP9", "MAC-10", "MP7", "MP5-SD", "UMP-45", "P90", "PP-Bizon", "CZ75-Auto",
	};

	readonly MainViewModel vm = new();

	public event Action? GalleryRequested;
	public event Action? ImportRequested;

	public MainView() {
		InitializeComponent();

		Cfg cfg = CfgStore.Cur;
		vm.Sens = cfg.Sens ?? float.NaN;
		vm.MYaw = cfg.MYaw;
		vm.CaptureMode = (int)cfg.CaptureMode;

		cmbCapture.ItemsSource = new[] { "LMB", "LMB+RMB (Combined)" };
		cmbCapture.SelectedIndex = vm.CaptureMode;
		cmbWeapon.ItemsSource = WEAPON_NAMES;
		cmbWeapon.SelectedIndex = 0;
		numMYaw.Value = Math.Round(vm.MYaw, 6);
		numThresh.Value = vm.Thresh;
		numSens.Value = float.IsNaN(vm.Sens) ? double.NaN : Math.Round(vm.Sens, 6);

		vm.PropertyChanged += (s, e) => onSprayChanged();

		Abi.slSetCaptureMode(vm.CaptureMode);
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
		CfgStore.Cur.Sens = float.IsNaN(vm.Sens) ? null : vm.Sens;
		CfgStore.Save();

		arm();
	}

	void onMYawChanged(NumberBox sender, NumberBoxValueChangedEventArgs e) {
		vm.MYaw = (float)sender.Value;
		CfgStore.Cur.MYaw = vm.MYaw;
		CfgStore.Save();

		arm();
	}

	void onWeaponChanged(object sender, SelectionChangedEventArgs e) {
		vm.SelectedWeapon = (Weapon)cmbWeapon.SelectedIndex;

		arm();
	}

	void onGallery(object sender, RoutedEventArgs e) {
		GalleryRequested?.Invoke();
	}

	void onImport(object sender, RoutedEventArgs e) {
		ImportRequested?.Invoke();
	}

	void onCaptureChanged(object sender, SelectionChangedEventArgs e) {
		vm.CaptureMode = cmbCapture.SelectedIndex;
		CfgStore.Cur.CaptureMode = (CaptureMode)vm.CaptureMode;
		CfgStore.Save();

		Abi.slSetCaptureMode(vm.CaptureMode);
	}
}
