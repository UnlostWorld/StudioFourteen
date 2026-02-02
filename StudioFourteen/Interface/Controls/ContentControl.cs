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

using Avalonia;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
using StudioFourteen.Services.Avalonia;

public class ContentControl : ContentPresenter
{
	public static readonly StyledProperty<string> PathProperty;
	public static readonly StyledProperty<object?> ContentDataContextProperty;

	private AvaloniaContentReference<Visual>? contentReference;

	static ContentControl()
	{
		PathProperty = AvaloniaProperty.Register<ContentControl, string>(nameof(ContentControl.Path));
		ContentDataContextProperty = AvaloniaProperty.Register<ContentControl, object?>(nameof(ContentControl.ContentDataContext));
	}

	public string Path
	{
		get => this.GetValue(PathProperty);
		set => this.SetValue(PathProperty, value);
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
		base.OnPropertyChanged(change);

		if (change.Property == PathProperty)
		{
			this.UpdateReference();
		}
		else if (change.Property == ContentDataContextProperty)
		{
			this.OnReferenceReloaded();
		}
	}

	private void UpdateReference()
	{
		this.contentReference?.Reloaded -= this.OnReferenceReloaded;
		this.contentReference?.Dispose();

		if (string.IsNullOrEmpty(this.Path))
			return;

		this.contentReference = new(this.Path);
		this.contentReference?.Reloaded += this.OnReferenceReloaded;
		this.OnReferenceReloaded();
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