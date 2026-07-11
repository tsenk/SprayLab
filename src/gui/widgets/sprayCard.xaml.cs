using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using SprayLab.Bindings;
using Windows.System;

namespace SprayLab.Widgets;

public sealed partial class SprayCard : UserControl {
	public Spray Spray { get; }

	public bool Checked => chk.IsChecked==true;

	public event Action<Spray>? OpenRequested;
	public event Action<SprayCard>? DeleteRequested;
	public event Action<SprayCard, string>? RenameRequested;

	public SprayCard(Spray sp, byte[] thumbPng) {
		InitializeComponent();

		Spray = sp;
		txtName.Text = sp.Name;

		using var ms = new MemoryStream(thumbPng);
		var bmp = new BitmapImage();
		bmp.SetSource(ms.AsRandomAccessStream());
		thumb.Source = bmp;
	}

	public void ApplyName(string name) {
		txtName.Text = name;
	}

	void onOpen(object sender, RoutedEventArgs e) {
		OpenRequested?.Invoke(Spray);
	}

	void onDelete(object sender, RoutedEventArgs e) {
		DeleteRequested?.Invoke(this);
	}

	void onNameKey(object sender, KeyRoutedEventArgs e) {
		if (e.Key==VirtualKey.Enter)
			commitName();
	}

	void onNameCommit(object sender, RoutedEventArgs e) {
		commitName();
	}

	void commitName() {
		string wanted = txtName.Text.Trim();

		if (wanted.Length==0 || wanted==Spray.Name) {
			txtName.Text = Spray.Name;
			return;
		}

		RenameRequested?.Invoke(this, wanted);
	}
}
