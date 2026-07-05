using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using SprayLab.Bindings;

namespace SprayLab.Widgets;

public sealed partial class SprayGraph : UserControl {
	const int DOT_R = 5;

	IReadOnlyList<Pos> refDots = Array.Empty<Pos>();
	IReadOnlyList<Pos> actualDots = Array.Empty<Pos>();

	public SprayGraph() {
		InitializeComponent();

		SizeChanged += (s, e) => render();
	}

	public void Show(IReadOnlyList<Pos> refPattern, IReadOnlyList<Pos> actual) {
		refDots = refPattern;
		actualDots = actual;

		render();
	}

	void render() {
		int w = (int)ActualWidth;
		int h = (int)ActualHeight;
		if (w<=0 || h<=0 || (refDots.Count==0 && actualDots.Count==0))
			return;

		var model = new PlotModel { Background = OxyColors.Transparent, PlotAreaBorderThickness = new OxyThickness(0) };

		// actual is raw counts until grading lands, so each series gets its own hidden space
		model.Axes.Add(new LinearAxis { Key = "refX", Position = AxisPosition.Bottom, IsAxisVisible = false });
		model.Axes.Add(new LinearAxis { Key = "refY", Position = AxisPosition.Left, StartPosition = 1, EndPosition = 0, IsAxisVisible = false });
		model.Axes.Add(new LinearAxis { Key = "actualX", Position = AxisPosition.Bottom, IsAxisVisible = false });
		model.Axes.Add(new LinearAxis { Key = "actualY", Position = AxisPosition.Left, StartPosition = 1, EndPosition = 0, IsAxisVisible = false });

		var refSeries = new ScatterSeries { XAxisKey = "refX", YAxisKey = "refY", MarkerType = MarkerType.Circle, MarkerSize = DOT_R, MarkerFill = OxyColor.FromRgb(0x34, 0xC7, 0x59) };
		foreach (Pos p in refDots)
			refSeries.Points.Add(new ScatterPoint(p.X, p.Y));

		var actualSeries = new ScatterSeries { XAxisKey = "actualX", YAxisKey = "actualY", MarkerType = MarkerType.Circle, MarkerSize = DOT_R, MarkerFill = OxyColor.FromRgb(0x29, 0x62, 0xFF) };
		foreach (Pos p in actualDots)
			actualSeries.Points.Add(new ScatterPoint(p.X, p.Y));

		model.Series.Add(refSeries);
		model.Series.Add(actualSeries);

		var exporter = new OxyPlot.SkiaSharp.PngExporter { Width = w, Height = h };
		using var ms = new MemoryStream();
		exporter.Export(model, ms);
		ms.Position = 0;

		var bmp = new BitmapImage();
		bmp.SetSource(ms.AsRandomAccessStream());

		img.Source = bmp;
	}
}
