using Microsoft.UI.Xaml;
using SprayLab.Bindings;

namespace SprayLab;

public partial class App : Application {
	const int PING_SENTINEL = 1337;

	Window? window;

	public App() {
		InitializeComponent();
	}

	protected override void OnLaunched(LaunchActivatedEventArgs args) {
		if (Abi.slPing()!=PING_SENTINEL)
			throw new InvalidOperationException("abi ping mismatch");

		window = new MainWindow();
		window.Activate();
	}
}
