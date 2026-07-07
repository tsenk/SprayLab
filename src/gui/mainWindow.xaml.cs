using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using SprayLab.Bindings;

namespace SprayLab;

public sealed partial class MainWindow : Window {
	// rooted so the native side never calls a collected delegate
	static Abi.SprayCallback? keepCb;

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
		Abi.slRegisterCapture();
	}

	void onSpray(IntPtr spray) {
		Spray sp = Abi.ReadSpray(spray);

		DispatcherQueue.TryEnqueue(() => mainView.ShowSpray(sp));
	}
}
