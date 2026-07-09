using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using SprayLab.Bindings;

namespace SprayLab.Widgets;

public sealed partial class SprayCard : UserControl {
	public Spray Spray { get; }

	public event Action<Spray>? OpenRequested;

	public SprayCard(Spray sp, byte[] thumbPng) {
		InitializeComponent();

		Spray = sp;
		txtName.Text = sp.Name;

		using var ms = new MemoryStream(thumbPng);
		var bmp = new BitmapImage();
		bmp.SetSource(ms.AsRandomAccessStream());
		thumb.Source = bmp;
	}

	void onOpen(object sender, RoutedEventArgs e) {
		OpenRequested?.Invoke(Spray);
	}
}
