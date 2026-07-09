using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using SprayLab.Bindings;

namespace SprayLab.Widgets;

public sealed partial class SprayGraph : UserControl {
	const int DOT_R = 5;

	IReadOnlyList<Bullet> bullets = Array.Empty<Bullet>();
	IReadOnlyList<Delta> flagged = Array.Empty<Delta>();

	public SprayGraph() {
		InitializeComponent();

		SizeChanged += (s, e) => render();
	}

	public void Show(IReadOnlyList<Bullet> shown, IReadOnlyList<Delta> mistakes) {
		bullets = shown;
		flagged = mistakes;

		render();
	}

	void render() {
		int w = (int)ActualWidth;
		int h = (int)ActualHeight;
		if (w<=0 || h<=0 || bullets.Count==0)
			return;

		byte[] png = RenderPng(bullets, flagged, w, h, DOT_R);

		using var ms = new MemoryStream(png);
		var bmp = new BitmapImage();
		bmp.SetSource(ms.AsRandomAccessStream());

		img.Source = bmp;
	}

	// pure model build and export, safe off the ui thread, thumbnails pass dotR 3 and no mistakes
	public static byte[] RenderPng(IReadOnlyList<Bullet> bullets, IReadOnlyList<Delta> flagged, int w, int h, int dotR) {
		float xMin = float.MaxValue;
		float xMax = float.MinValue;
		float yMin = float.MaxValue;
		float yMax = float.MinValue;
		foreach (Bullet b in bullets) {
			xMin = MathF.Min(xMin, MathF.Min(b.Actual.X, b.Ref.X));
			xMax = MathF.Max(xMax, MathF.Max(b.Actual.X, b.Ref.X));
			yMin = MathF.Min(yMin, MathF.Min(b.Actual.Y, b.Ref.Y));
			yMax = MathF.Max(yMax, MathF.Max(b.Actual.Y, b.Ref.Y));
		}

		float xPad = MathF.Max((xMax-xMin)*0.1f, 0.5f);
		float yPad = MathF.Max((yMax-yMin)*0.1f, 0.5f);
		xMin -= xPad;
		xMax += xPad;
		yMin -= yPad;
		yMax += yPad;

		var model = new PlotModel { Background = OxyColors.Transparent, PlotAreaBorderThickness = new OxyThickness(0), PlotMargins = new OxyThickness(0), Padding = new OxyThickness(0) };

		model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Minimum = xMin, Maximum = xMax, IsAxisVisible = false });
		model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Minimum = yMin, Maximum = yMax, StartPosition = 1, EndPosition = 0, IsAxisVisible = false });

		// zero overlap gate in canvas px, geometric, separate from the degree threshold
		float scaleX = w/(xMax-xMin);
		float scaleY = h/(yMax-yMin);
		int shownNum = 1;
		foreach (Delta d in flagged) {
			Bullet b = bullets[d.Num];

			float dxPx = (b.Actual.X-b.Ref.X)*scaleX;
			float dyPx = (b.Actual.Y-b.Ref.Y)*scaleY;
			if (MathF.Sqrt(dxPx*dxPx+dyPx*dyPx)>2*dotR) {
				var line = new LineSeries { Color = OxyColor.FromRgb(0xE5, 0x3E, 0x3E), StrokeThickness = 1.5 };
				line.Points.Add(new DataPoint(b.Actual.X, b.Actual.Y));
				line.Points.Add(new DataPoint(b.Ref.X, b.Ref.Y));
				model.Series.Add(line);
			}

			model.Annotations.Add(new TextAnnotation {
				Text = shownNum.ToString(),
				TextPosition = new DataPoint(b.Actual.X-12/scaleX, b.Actual.Y),
				TextColor = OxyColors.White,
				FontSize = 12,
				StrokeThickness = 0,
			});
			shownNum++;
		}

		var refSeries = new ScatterSeries { MarkerType = MarkerType.Circle, MarkerSize = dotR, MarkerFill = OxyColor.FromRgb(0x34, 0xC7, 0x59) };
		var actualSeries = new ScatterSeries { MarkerType = MarkerType.Circle, MarkerSize = dotR, MarkerFill = OxyColor.FromRgb(0x29, 0x62, 0xFF) };
		foreach (Bullet b in bullets) {
			refSeries.Points.Add(new ScatterPoint(b.Ref.X, b.Ref.Y));
			actualSeries.Points.Add(new ScatterPoint(b.Actual.X, b.Actual.Y));
		}

		model.Series.Add(refSeries);
		model.Series.Add(actualSeries);

		var exporter = new OxyPlot.SkiaSharp.PngExporter { Width = w, Height = h };
		using var ms = new MemoryStream();
		exporter.Export(model, ms);

		return ms.ToArray();
	}
}
