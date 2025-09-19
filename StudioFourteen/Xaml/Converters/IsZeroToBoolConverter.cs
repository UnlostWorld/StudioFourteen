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

namespace StudioFourteen.Xaml;

using System;
using System.Windows.Data;

[ValueConversion(typeof(object), typeof(bool))]
public class IsZeroToBoolConverter : IValueConverter
{
	public static bool IsZero(object value)
	{
		if (value is int intV)
		{
			return intV == 0;
		}
		else if (value is float floatV)
		{
			return floatV == 0;
		}
		else if (value is double doubleV)
		{
			return doubleV == 0;
		}
		else if (value is uint uintV)
		{
			return uintV == 0;
		}
		else if (value is ushort ushortV)
		{
			return ushortV == 0;
		}
		else if (value is byte byteV)
		{
			return byteV == 0;
		}
		else
		{
			throw new NotImplementedException($"value type {value.GetType()} not supported for not zero converter");
		}
	}

	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		return IsZeroToBoolConverter.IsZero(value);
	}

	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
