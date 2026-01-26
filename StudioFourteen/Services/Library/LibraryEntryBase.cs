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

namespace StudioFourteen.Services.Library;

using StudioFourteen.Services.Library.Tags;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public delegate void EntryEvent();

/// <summary>
/// An entry is a library object.
/// </summary>
public abstract class LibraryEntryBase : ITagged, INotifyPropertyChanged////, IDraggable
{
	private readonly SourceBase? source;

	public LibraryEntryBase(SourceBase? source)
	{
		this.source = source;
	}

	public event PropertyChangedEventHandler? PropertyChanged;
	public event EntryEvent? ExecuteRequested;

	public virtual IComparable DefaultSortValue => this.Name ?? this.Identifier;
	public abstract string? Name { get; }
	public abstract string? SubTitle { get; }
	public virtual object? Icon => null; ////XamlResources.Find("ICON_Library_Entry");

	public virtual bool IsVisible { get; set; }
	public TagCollection Tags { get; init; } = new();
	public SourceBase? Source => this.source;

	public virtual bool IsValid => true;

	public string Identifier => $"{this.Source?.GetInternalId()}||{this.GetInternalId()}";

	public bool IsFavorite
	{
		get => LibraryFavoritesFilter.GetIsFavorite(this);
		set
		{
			LibraryFavoritesFilter.SetIsFavorite(this, value);
			this.NotifyPropertyChanged();
		}
	}

	////public virtual object? GetDragPreviewContent() => this.Icon;
	////public virtual IDragSceneInstance? CreateSceneInstance() => null;

	public virtual Task Execute()
	{
		return Task.CompletedTask;
	}

	public virtual bool IsType(Type type) => this.GetType().IsAssignableTo(type);
	public virtual bool Search(string[] query) => SearchUtility.Matches(this.Name, query);

	public virtual void Dispose()
	{
	}

	public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public virtual LibraryPreviewBase? GetPreview()
	{
		return null;
	}

	protected abstract string GetInternalId();
}