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

namespace StudioFourteen.Interface.Controls;

using System;
using Avalonia;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Lumina.Data.Structs.Excel;
using StudioFourteen.Services.Avalonia;

public class ContentControl : ContentPresenter
{
	public static readonly StyledProperty<string?> ContentPathProperty;
	public static readonly StyledProperty<object?> ContentDataContextProperty;

	private AvaloniaContentReference<Visual>? contentReference;

	static ContentControl()
	{
		ContentPathProperty = AvaloniaProperty.Register<ContentControl, string?>(nameof(ContentControl.ContentPath));
		ContentDataContextProperty = AvaloniaProperty.Register<ContentControl, object?>(nameof(ContentControl.ContentDataContext));
	}

	public string? ContentPath
	{
		get => this.GetValue(ContentPathProperty);
		set => this.SetValue(ContentPathProperty, value);
	}

	public object? ContentDataContext
	{
		get => this.GetValue(ContentDataContextProperty);
		set => this.SetValue(ContentDataContextProperty, value);
	}

	protected override void OnLoaded(RoutedEventArgs e)
	{
		base.OnLoaded(e);

		Visual? v = this.Content as Visual;
		if (v != null)
		{
			v.DataContext = this.ContentDataContext ?? this.DataContext;
		}
	}

	protected override void OnUnloaded(RoutedEventArgs e)
	{
		base.OnUnloaded(e);

		this.contentReference?.Reloaded -= this.OnReferenceReloaded;
		this.contentReference?.Dispose();
		this.contentReference = null;
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		try
		{
			if (change.Property == ContentPathProperty)
			{
				this.UpdateReference();
			}
			else if (change.Property == ContentDataContextProperty)
			{
				Visual? v = this.Content as Visual;
				if (v != null)
				{
					v.DataContext = this.ContentDataContext ?? this.DataContext;
				}
			}

			base.OnPropertyChanged(change);
		}
		catch (Exception ex)
		{
			Studio.Log.Error(ex, "Error in property changed");
			this.Content = new Svg()
			{
				Source = "Icons/Warning.svg",
				Foreground = new SolidColorBrush(Colors.DarkRed),
			};
		}
	}

	private void UpdateReference()
	{
		this.contentReference?.Reloaded -= this.OnReferenceReloaded;
		this.contentReference?.Dispose();

		if (string.IsNullOrEmpty(this.ContentPath))
			return;

		this.contentReference = new(this.ContentPath);
		this.contentReference.Reloaded += this.OnReferenceReloaded;

		Visual? content = this.contentReference.Get();
		if (content == null)
			throw new Exception("Failed to get content");

		content.DataContext = this.ContentDataContext ?? this.DataContext;
		this.Content = content;
	}

	private void OnReferenceReloaded()
	{
		Studio.Tick.Dispatch(Services.Tick.TickChannels.Ui, () =>
		{
			Visual? v = this.contentReference?.Get();
			v?.DataContext = this.ContentDataContext ?? this.DataContext;
			this.Content = v;
		});
	}
}