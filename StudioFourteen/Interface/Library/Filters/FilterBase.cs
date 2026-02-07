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

namespace StudioFourteen.Interface.Library.Filters;

using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using StudioFourteen.Interface.Controls;
using StudioFourteen.Services.Library;

public abstract class FilterBase : TemplatedControl, ILibraryFilter
{
	private LibraryInspector? library;

	public abstract void Freeze();
	public abstract bool Filter(LibraryEntryBase entry);

	protected override void OnLoaded(RoutedEventArgs e)
	{
		base.OnLoaded(e);

		this.library = this.FindAncestorOfType<LibraryInspector>();
		this.library?.AddFilter(this);
	}

	protected override void OnUnloaded(RoutedEventArgs e)
	{
		base.OnUnloaded(e);

		this.library?.RemoveFilter(this);
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		base.OnPropertyChanged(change);

		if (this.IsFilterProperty(change.Property))
		{
			this.ApplyFilters();
		}
	}

	protected virtual bool IsFilterProperty(AvaloniaProperty property)
	{
		return false;
	}

	protected void ApplyFilters()
	{
		this.library?.Search();
	}
}
