using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SprayLab.Bindings;

namespace SprayLab.Gallery;

public sealed partial class GalleryView : UserControl {
	public readonly GalleryViewModel Vm = new();

	public event Action? BackRequested;
	public event Action<Spray>? OpenRequested;
	public event Action<List<Widgets.SprayCard>>? DeleteSelectedRequested;

	public GalleryView() {
		InitializeComponent();

		days.ItemsSource = Vm.Days;
		Vm.OpenRequested += sp => OpenRequested?.Invoke(sp);
	}

	void onBack(object sender, RoutedEventArgs e) {
		BackRequested?.Invoke();
	}

	void onDeleteSelected(object sender, RoutedEventArgs e) {
		var checkedCards = Vm.CheckedCards();
		if (checkedCards.Count>0)
			DeleteSelectedRequested?.Invoke(checkedCards);
	}
}
