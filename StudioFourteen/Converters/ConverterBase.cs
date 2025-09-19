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

using System;
using System.Globalization;
using System.Windows.Data;

public abstract class ConverterBase<TFrom, TTo> : IValueConverter
{
	public object? Parameter { get; private set; }

	public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		this.Parameter = parameter;

		if (value is TFrom tValue)
			return this.Convert(tValue);

		throw new InvalidCastException();
	}

	public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		this.Parameter = parameter;

		if (value is TTo tValue)
			return this.ConvertBack(tValue);

		throw new InvalidCastException();
	}

	protected abstract TTo Convert(TFrom? value);

	protected virtual TFrom ConvertBack(TTo? value)
	{
		throw new NotSupportedException();
	}
}

public abstract class ConverterBase<TFrom, TTo, TParameter> : ConverterBase<TFrom, TTo>
{
	public new TParameter Parameter
	{
		get
		{
			if (base.Parameter is TParameter tParameter)
				return tParameter;

			if (typeof(TParameter) == typeof(double))
			{
				double val = System.Convert.ToDouble(base.Parameter, CultureInfo.InvariantCulture);

				if (val is TParameter tParameterVal)
					return tParameterVal;
			}

			if (typeof(TParameter) == typeof(int))
			{
				int val = System.Convert.ToInt32(base.Parameter, CultureInfo.InvariantCulture);

				if (val is TParameter tParameterVal)
					return tParameterVal;
			}

			throw new InvalidCastException();
		}
	}
}
