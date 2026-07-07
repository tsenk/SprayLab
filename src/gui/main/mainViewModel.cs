using System.ComponentModel;
using SprayLab.Bindings;

namespace SprayLab.Main;

public class MainViewModel : INotifyPropertyChanged {
	public float Sens = float.NaN;
	public float MYaw = 0.022f;
	public Weapon SelectedWeapon = Weapon.Ak47;
	public int CaptureMode;
	public float Thresh = 1.0f;

	Spray? lastSpray;

	public Spray? LastSpray {
		get => lastSpray;
		set {
			lastSpray = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastSpray)));
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;
}
