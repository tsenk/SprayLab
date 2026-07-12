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
	IReadOnlyList<Pos> refPattern = Array.Empty<Pos>();

	public SprayGraph() {
		InitializeComponent();

		SizeChanged += (s, e) => render();
		ActualThemeChanged += (s, e) => render();
	}

	public void Show(IReadOnlyList<Bullet> shown, IReadOnlyList<Delta> mistakes, IReadOnlyList<Pos> pattern) {
		bullets = shown;
		flagged = mistakes;
		refPattern = pattern;

		render();
	}

	void render() {
		int w = (int)ActualWidth;
		int h = (int)ActualHeight;
		if (w<=0 || h<=0 || bullets.Count==0)
			return;

		OxyColor numColor = ActualTheme==Microsoft.UI.Xaml.ElementTheme.Light ? OxyColors.Black : OxyColors.White;
		byte[] png = RenderPng(bullets, flagged, refPattern, w, h, DOT_R, numColor);

		using var ms = new MemoryStream(png);
		var bmp = new BitmapImage();
		bmp.SetSource(ms.AsRandomAccessStream());

		img.Source = bmp;
	}

	// pure model build and export, safe off the ui thread, thumbnails pass dotR 3 and no mistakes
	// the full pattern always draws so scaling holds across spray lengths
	public static byte[] RenderPng(IReadOnlyList<Bullet> bullets, IReadOnlyList<Delta> flagged, IReadOnlyList<Pos> refPattern, int w, int h, int dotR, OxyColor numColor) {
		float xMin = float.MaxValue;
		float xMax = float.MinValue;
		float yMin = float.MaxValue;
		float yMax = float.MinValue;
		foreach (Bullet b in bullets) {
			xMin = MathF.Min(xMin, b.Actual.X);
			xMax = MathF.Max(xMax, b.Actual.X);
			yMin = MathF.Min(yMin, b.Actual.Y);
			yMax = MathF.Max(yMax, b.Actual.Y);
		}
		foreach (Pos p in refPattern) {
			xMin = MathF.Min(xMin, p.X);
			xMax = MathF.Max(xMax, p.X);
			yMin = MathF.Min(yMin, p.Y);
			yMax = MathF.Max(yMax, p.Y);
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
				TextColor = numColor,
				FontSize = 12,
				StrokeThickness = 0,
			});
			shownNum++;
		}

		var refSeries = new ScatterSeries { MarkerType = MarkerType.Circle, MarkerSize = dotR, MarkerFill = OxyColor.FromRgb(0x34, 0xC7, 0x59) };
		foreach (Pos p in refPattern)
			refSeries.Points.Add(new ScatterPoint(p.X, p.Y));

		var actualSeries = new ScatterSeries { MarkerType = MarkerType.Circle, MarkerSize = dotR, MarkerFill = OxyColor.FromRgb(0x29, 0x62, 0xFF) };
		foreach (Bullet b in bullets)
			actualSeries.Points.Add(new ScatterPoint(b.Actual.X, b.Actual.Y));

		// ref on top so bullet 1 of the pattern stays visible at the shared 0,0 start
		model.Series.Add(actualSeries);
		model.Series.Add(refSeries);

		var exporter = new OxyPlot.SkiaSharp.PngExporter { Width = w, Height = h };
		using var ms = new MemoryStream();
		exporter.Export(model, ms);

		return ms.ToArray();
	}
}
