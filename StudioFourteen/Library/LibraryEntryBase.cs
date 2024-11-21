// .                      @@             _____ _______ _    _ _____ _____ ____
//            @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//           @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//           @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//          @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//      @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//       @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//        @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//        @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//      @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//       @@@@             @@@
//         @@@@@      @@@@@               This software is licensed under the
//          @@@@@@@@@@@@@@                 GNU AFFERO GENERAL PUBLIC LICENSE
//              @@@@  @                       Version 3, 19 November 2007

namespace StudioFourteen.Library;

using Serilog;
using StudioFourteen.Library.LibraryMenu;
using StudioFourteen.Library.Sources;
using StudioFourteen.Tags;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using WpfUtils;

public delegate void EntryEvent();

/// <summary>
/// An entry is a library object.
/// </summary>
public abstract class LibraryEntryBase : ITagged, INotifyPropertyChanged
{
	protected readonly ILogger Log;

	private readonly SourceBase? source;

	public LibraryEntryBase(SourceBase? source)
	{
		this.source = source;
		this.Log = Logging.ForContext(this.GetType());
	}

	public event PropertyChangedEventHandler? PropertyChanged;
	public event EntryEvent? ExecuteRequested;

	public ServiceManager Services => ServiceManager.Instance;

	public abstract string? Name { get; }
	public abstract string? SubTitle { get; }

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

	public virtual bool IsType(Type type) => this.GetType().IsAssignableTo(type);
	public virtual bool Search(string[] query) => SearchUtility.Matches(this.Name, query);

	public virtual void Dispose()
	{
	}

	public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public virtual async Task<List<MenuEntry>> GetLibraryMenus()
	{
		List<MenuEntry> menus = new();

		Type type = this.GetType();
		MethodInfo[] methods = type.GetMethods();
		foreach (MethodInfo method in methods)
		{
			LibraryMenuAttributeBase? attribute = method.GetCustomAttribute<LibraryMenuAttributeBase>();
			if (attribute == null)
				continue;

			menus.AddRange(await attribute.GetMenu(this, method));
		}

		return menus;
	}

	protected abstract string GetInternalId();
}