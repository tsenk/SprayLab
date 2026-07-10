using System.Runtime.InteropServices;
using System.Text;
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
	bool importing = false;

	[DllImport("user32.dll")]
	static extern uint GetDpiForWindow(IntPtr hwnd);

	[DllImport("comdlg32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	static extern bool GetOpenFileNameW(ref Openfilename ofn);

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	struct Openfilename {
		public int StructSize;
		public IntPtr Owner;
		public IntPtr Instance;
		public string Filter;
		public string? CustomFilter;
		public int MaxCustFilter;
		public int FilterIndex;
		public IntPtr File;
		public int MaxFile;
		public string? FileTitle;
		public int MaxFileTitle;
		public string? InitialDir;
		public string? Title;
		public int Flags;
		public short FileOffset;
		public short FileExtension;
		public string? DefExt;
		public IntPtr CustData;
		public IntPtr Hook;
		public string? TemplateName;
		public IntPtr Reserved0;
		public int Reserved1;
		public int FlagsEx;
	}

	public MainWindow() {
		InitializeComponent();

		// ResizeClient takes physical px
		var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
		double scale = GetDpiForWindow(hwnd)/96.0;
		AppWindow.ResizeClient(new Windows.Graphics.SizeInt32((int)(1100*scale), (int)(700*scale)));

		mainView.GalleryRequested += () => showGallery(true);
		mainView.ImportRequested += importSprays;
		galleryView.BackRequested += () => showGallery(false);
		galleryView.OpenRequested += sp => {
			mainView.ShowSpray(sp);
			showGallery(false);
		};
		galleryView.Vm.DeleteRequested += deleteSpray;
		galleryView.Vm.RenameRequested += renameSpray;
		galleryView.DeleteSelectedRequested += cards => {
			foreach (SprayCard card in cards)
				deleteSpray(card);
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

	void deleteSpray(SprayCard card) {
		if (Abi.slDeleteSpray(card.Spray.Name)!=1)
			return;

		sprays.Remove(card.Spray);
		galleryView.Vm.RemoveCard(card);
	}

	void renameSpray(SprayCard card, string wanted) {
		var final = new StringBuilder(256);

		if (Abi.slRenameSpray(card.Spray.Name, wanted, final, final.Capacity)==1)
			card.Spray.Name = final.ToString();

		card.ApplyName(card.Spray.Name);
	}

	void importSprays() {
		var buf = Marshal.AllocHGlobal(65536);
		try {
			// zeroed multiselect buffer, result is dir\0file\0file\0\0 or a single full path
			for (int i = 0; i<65536; i++)
				Marshal.WriteByte(buf, i, 0);

			var ofn = new Openfilename {
				StructSize = Marshal.SizeOf<Openfilename>(),
				Owner = WinRT.Interop.WindowNative.GetWindowHandle(this),
				Filter = "Spray files (*.spray)\0*.spray\0\0",
				File = buf,
				MaxFile = 32768,
				// OFN_EXPLORER | OFN_ALLOWMULTISELECT | OFN_FILEMUSTEXIST
				Flags = 0x00080000|0x00000200|0x00001000,
			};

			if (!GetOpenFileNameW(ref ofn))
				return;

			var parts = new List<string>();
			int at = 0;
			while (true) {
				string s = Marshal.PtrToStringUni(IntPtr.Add(buf, at*2)) ?? "";
				if (s.Length==0)
					break;

				parts.Add(s);
				at += s.Length+1;
			}

			var paths = new List<string>();
			if (parts.Count==1)
				paths.Add(parts[0]);
			else
				for (int i = 1; i<parts.Count; i++)
					paths.Add(Path.Combine(parts[0], parts[i]));

			importing = true;
			foreach (string path in paths)
				Abi.slImportSpray(path);
			importing = false;
		} finally {
			Marshal.FreeHGlobal(buf);
		}
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

		if (importing) {
			sprays.Add(sp);
			galleryView.Vm.AddCard(sp, SprayGraph.RenderPng(sp.Bullets, Array.Empty<Delta>(), THUMB_W, THUMB_H, THUMB_DOT_R));
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
