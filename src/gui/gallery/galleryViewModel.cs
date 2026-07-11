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
	public event Action<SprayCard>? DeleteRequested;
	public event Action<SprayCard, string>? RenameRequested;

	public SprayCard? CardOf(Spray sp) {
		foreach (DayGroup g in Days) {
			SprayCard? card = g.Cards.FirstOrDefault(c => c.Spray==sp);
			if (card!=null)
				return card;
		}

		return null;
	}

	public List<Spray> FlatSprays() {
		var list = new List<Spray>();

		foreach (DayGroup g in Days)
			list.AddRange(g.Cards.Select(c => c.Spray));

		return list;
	}

	public List<SprayCard> CheckedCards() {
		var list = new List<SprayCard>();

		foreach (DayGroup g in Days)
			list.AddRange(g.Cards.Where(c => c.Checked));

		return list;
	}

	public void RemoveCard(SprayCard card) {
		foreach (DayGroup g in Days) {
			if (!g.Cards.Remove(card))
				continue;

			if (g.Cards.Count==0)
				Days.Remove(g);
			return;
		}
	}

	public void AddCard(Spray sp, byte[] thumbPng) {
		var card = new SprayCard(sp, thumbPng);
		card.OpenRequested += s => OpenRequested?.Invoke(s);
		card.DeleteRequested += c => DeleteRequested?.Invoke(c);
		card.RenameRequested += (c, name) => RenameRequested?.Invoke(c, name);

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
