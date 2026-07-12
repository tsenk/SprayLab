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
}
