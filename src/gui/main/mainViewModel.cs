using System.ComponentModel;
using SprayLab.Bindings;

namespace SprayLab.Main;

public class MainViewModel : INotifyPropertyChanged {
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
