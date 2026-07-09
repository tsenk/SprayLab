using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using SprayLab.Bindings;
using SprayLab.Widgets;

namespace SprayLab;

public sealed partial class MainWindow : Window {
	const int THUMB_W = 134;
	const int THUMB_H = 168;
	const int THUMB_DOT_R = 3;

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

		mainView.GalleryRequested += () => showGallery(true);
		galleryView.BackRequested += () => showGallery(false);
		galleryView.OpenRequested += sp => {
			mainView.ShowSpray(sp);
			showGallery(false);
		};

		keepCb = onSpray;
		Abi.slSetSprayCallback(keepCb);

		Abi.slInit();
		loading = false;

		if (sprays.Count>0)
			mainView.ShowSpray(sprays[^1]);

		prerenderCards();

		Abi.slRegisterCapture();
	}

	void showGallery(bool open) {
		if (open)
			galleryView.Vm.RefreshTitles();

		mainView.Visibility = open ? Visibility.Collapsed : Visibility.Visible;
		galleryView.Visibility = open ? Visibility.Visible : Visibility.Collapsed;
	}

	// gallery must open instantly, cards are built ahead of time after startup
	void prerenderCards() {
		Spray[] snapshot = sprays.ToArray();

		Task.Run(() => {
			foreach (Spray sp in snapshot) {
				byte[] png = SprayGraph.RenderPng(sp.Bullets, Array.Empty<Delta>(), THUMB_W, THUMB_H, THUMB_DOT_R);

				DispatcherQueue.TryEnqueue(() => galleryView.Vm.AddCard(sp, png));
			}
		});
	}

	void onSpray(IntPtr spray) {
		Spray sp = Abi.ReadSpray(spray);

		// load path runs synchronously on the ui thread, live captures come from the capture thread
		if (loading) {
			sprays.Add(sp);
			return;
		}

		byte[] png = SprayGraph.RenderPng(sp.Bullets, Array.Empty<Delta>(), THUMB_W, THUMB_H, THUMB_DOT_R);

		DispatcherQueue.TryEnqueue(() => {
			sprays.Add(sp);
			mainView.ShowSpray(sp);
			galleryView.Vm.AddCard(sp, png);
		});
	}
}
