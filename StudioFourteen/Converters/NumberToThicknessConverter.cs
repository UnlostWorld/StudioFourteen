// .                    @@             _____ _______ _    _ _____ _____ ____
//          @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//         @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//         @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//        @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//    @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//     @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//      @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//      @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//    @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//     @@@@             @@@        https://github.com/UnlostWorld/StudioFourteen
//       @@@@@      @@@@@
//        @@@@@@@@@@@@@@                This software is licensed under the
//            @@@@  @                  GNU AFFERO GENERAL PUBLIC LICENSE v3

namespace StudioFourteen.Converters;

using System.Windows;

public abstract class NumberToThicknessConverter : ConverterBase<double, Thickness>
{
	private static readonly ThicknessConverter ThicknessConverter = new();

	protected sealed override Thickness Convert(double value)
	{
		Thickness thickness = default;

		if (this.Parameter is string paramStr)
		{
			object? obj = ThicknessConverter.ConvertFrom(this.Parameter);
			if (obj is Thickness thicknessParam)
			{
				thickness = thicknessParam;
			}
		}

		this.Add(value, ref thickness);
		return thickness;
	}

	protected abstract void Add(double value, ref Thickness thickness);
}

public class NumberToThicknessLeftConverter : NumberToThicknessConverter
{
	protected override void Add(double value, ref Thickness baseThickness)
	{
		baseThickness.Left += value;
	}
}

public class NumberToThicknessTopConverter : NumberToThicknessConverter
{
	protected override void Add(double value, ref Thickness baseThickness)
	{
		baseThickness.Top += value;
	}
}

public class NumberToThicknessRightConverter : NumberToThicknessConverter
{
	protected override void Add(double value, ref Thickness baseThickness)
	{
		baseThickness.Right += value;
	}
}

public class NumberToThicknessBottomConverter : NumberToThicknessConverter
{
	protected override void Add(double value, ref Thickness baseThickness)
	{
		baseThickness.Bottom += value;
	}
}