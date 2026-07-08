using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using SprayLab.Bindings;

namespace SprayLab;

public sealed partial class MainWindow : Window {
	// rooted so the native side never calls a collected delegate
	static Abi.SprayCallback? keepCb;

	readonly List<Spray> sprays = new();
	bool loading = true;

	[DllImport("user32.dll")]
	static extern uint GetDpiForWindow(IntPtr hwnd);

	public MainWindow() {
		InitializeComponent();

		// ResizeClient takes physical px
		var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
		double scale = GetDpiForWindow(hwnd)/96.0;
		AppWindow.ResizeClient(new Windows.Graphics.SizeInt32((int)(1100*scale), (int)(700*scale)));

		keepCb = onSpray;
		Abi.slSetSprayCallback(keepCb);

		Abi.slInit();
		loading = false;

		if (sprays.Count>0)
			mainView.ShowSpray(sprays[^1]);

		Abi.slRegisterCapture();
	}

	void onSpray(IntPtr spray) {
		Spray sp = Abi.ReadSpray(spray);

		// load path runs synchronously on the ui thread, live captures come from the capture thread
		if (loading) {
			sprays.Add(sp);
			return;
		}

		DispatcherQueue.TryEnqueue(() => {
			sprays.Add(sp);
			mainView.ShowSpray(sp);
		});
	}
}
