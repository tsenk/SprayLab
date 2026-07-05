using Microsoft.UI.Xaml;
using SprayLab.Bindings;

namespace SprayLab;

public sealed partial class MainWindow : Window {
	// rooted so the native side never calls a collected delegate
	static Abi.SprayCallback? keepCb;

	public MainWindow() {
		InitializeComponent();

		keepCb = onSpray;
		Abi.slSetSprayCallback(keepCb);
		Abi.slRegisterCapture();
		Abi.slSetWeapon((int)Weapon.Ak47);
	}

	void onSpray(IntPtr spray) {
		Spray sp = Abi.ReadSpray(spray);

		DispatcherQueue.TryEnqueue(() => mainView.ShowSpray(sp));
	}
}
