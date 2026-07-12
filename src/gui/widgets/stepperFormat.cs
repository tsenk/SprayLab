using Microsoft.UI.Xaml.Controls;
using Windows.Globalization.NumberFormatting;

namespace SprayLab.Widgets;

public static class StepperFormat {
	// spin buttons accumulate binary float noise without a rounder
	public static void Round(NumberBox box, double increment) {
		box.NumberFormatter = new DecimalFormatter {
			IntegerDigits = 1,
			FractionDigits = 0,
			NumberRounder = new IncrementNumberRounder { Increment = increment, RoundingAlgorithm = RoundingAlgorithm.RoundHalfUp },
		};
	}

	// numberbox only evaluates on focus loss, enter should submit too
	public static void CommitOnEnter(NumberBox box) {
		box.KeyDown += (s, e) => {
			if (e.Key!=Windows.System.VirtualKey.Enter)
				return;

			if (double.TryParse(box.Text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double v) || double.TryParse(box.Text, out v))
				box.Value = v;
		};
	}
}
