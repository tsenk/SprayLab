using System.Collections.ObjectModel;
using System.ComponentModel;
using SprayLab.Bindings;
using SprayLab.Widgets;

namespace SprayLab.Gallery;

public class DayGroup : INotifyPropertyChanged {
	public DateTime Day;

	string title = "";

	public string Title {
		get => title;
		set {
			title = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
		}
	}

	public ObservableCollection<SprayCard> Cards { get; } = new();

	public event PropertyChangedEventHandler? PropertyChanged;
}

public class GalleryViewModel {
	public ObservableCollection<DayGroup> Days { get; } = new();

	public event Action<Spray>? OpenRequested;

	public void AddCard(Spray sp, byte[] thumbPng) {
		var card = new SprayCard(sp, thumbPng);
		card.OpenRequested += s => OpenRequested?.Invoke(s);

		DateTime day = DateTimeOffset.FromUnixTimeSeconds(sp.Epoch).ToLocalTime().Date;

		DayGroup? group = Days.FirstOrDefault(g => g.Day==day);
		if (group==null) {
			group = new DayGroup { Day = day };

			int at = 0;
			while (at<Days.Count && Days[at].Day>day)
				at++;
			Days.Insert(at, group);
		}

		int slot = 0;
		while (slot<group.Cards.Count && group.Cards[slot].Spray.Epoch>sp.Epoch)
			slot++;
		group.Cards.Insert(slot, card);

		RefreshTitles();
	}

	// today is relative, recomputed whenever the gallery shows
	public void RefreshTitles() {
		DateTime today = DateTime.Now.Date;

		foreach (DayGroup g in Days)
			g.Title = g.Day==today ? "Today" : g.Day.ToString("yy-MM-dd");
	}
}
