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

namespace StudioFourteen.Themes;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

[ContentProperty(nameof(ThemeOptionsSet.Options))]
public class ThemeOptionsSet : IEnumerable
{
	private ThemeOption? currentOption;

	public string? Name { get; set; }
	public object? Key { get; set; }
	public List<ThemeOption> Options { get; set; } = new();

	public ThemeOption? CurrentOption
	{
		get
		{
			if (this.currentOption == null && this.Name != null)
			{
				ServiceManager.Instance.Settings.Current.ThemeOptions.TryGetValue(this.Name, out string? optionName);
				if (optionName != null)
				{
					foreach(ThemeOption option in this.Options)
					{
						if (option.Name == optionName)
						{
							this.CurrentOption = option;
							break;
						}
					}
				}
				else if (this.Options.Count > 0)
				{
					this.CurrentOption = this.Options[0];
				}
			}

			return this.currentOption;
		}

		set
		{
			this.currentOption = value;

			if (this.Name == null
				|| this.Key == null
				|| this.currentOption == null
				|| this.currentOption.Value == null
				|| this.currentOption.Name == null)
				return;

			ServiceManager.Instance.Settings.Current.ThemeOptions[this.Name] = this.currentOption.Name;
			ServiceManager.Instance.Settings.SaveImmediate();

			Resources.Set(this.Key, () =>
			{
				if (this.currentOption.Value == null)
					throw new InvalidOperationException();

				return this.currentOption.Value;
			});
		}
	}

	public IEnumerator GetEnumerator() => this.Options.GetEnumerator();
}

[ContentProperty(nameof(ThemeOption.Value))]
public class ThemeOption
{
	private object? value;
	public string? Name { get; set; }

	public object? Value
	{
		get => this.value;
		set
		{
			this.value = value;
			if (this.value is Freezable freezable)
			{
				freezable.Freeze();
			}
		}
	}
}