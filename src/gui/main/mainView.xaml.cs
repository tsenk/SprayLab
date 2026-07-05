using Microsoft.UI.Xaml.Controls;
using SprayLab.Bindings;

namespace SprayLab.Main;

public sealed partial class MainView : UserControl {
	readonly MainViewModel vm = new();
	readonly Pos[] refPattern;

	public MainView() {
		InitializeComponent();

		int intv = 0;
		var buf = new Pos[64];
		int n = Abi.slWeaponRef((int)Weapon.Ak47, ref intv, buf, buf.Length);
		refPattern = buf[..n];

		vm.PropertyChanged += (s, e) => render();
	}

	public void ShowSpray(Spray sp) {
		vm.LastSpray = sp;
	}

	void render() {
		if (vm.LastSpray==null)
			return;

		var actual = vm.LastSpray.Bullets.Select(b => b.Actual).ToList();

		graph.Show(refPattern, actual);
	}
}
