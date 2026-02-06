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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Metadata;
using CommunityToolkit.Mvvm.Input;
using PropertyGenerator.Avalonia;
using StudioFourteen.Interface.Library.Filters;
using StudioFourteen.Services.Library;
using StudioFourteen.Services.Library.Results;
using StudioFourteen.Services.Tick;

public partial class LibraryInspector : TemplatedControl
{
	private readonly List<LibraryEntryBase> entries = new();

	public LibraryInspector()
	{
		this.Filters.CollectionChanged += this.OnFiltersChanged;
	}

	[GeneratedStyledProperty(DefaultBindingMode = BindingMode.TwoWay)]
	public partial LibraryEntryBase? Value { get; set; }

	[GeneratedStyledProperty]
	public partial AvaloniaList<LibraryEntryBase> Entries { get; set; }

	[Content]
	public AvaloniaList<FilterBase> Filters { get; } = new();

	[RelayCommand]
	public void Search()
	{
		// TODO: Delay the search to wait for more changes
		Task.Run(async () => await this.SearchAsyncSafe());
	}

	private void OnFiltersChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		this.Search();

		foreach (FilterBase filter in this.Filters)
		{
			filter.PropertyChanged += (s, e) => this.Search();
		}
	}

	private async Task SearchAsyncSafe()
	{
		try
		{
			await this.SearchAsync();
		}
		catch (Exception ex)
		{
			Studio.Log.Error(ex, "Error searching library");
		}
	}

	private async Task SearchAsync()
	{
		await TickService.UiTick();
		IEnumerable<FilterBase> filters = this.Filters;
		foreach (FilterBase filter in filters)
		{
			filter.Freeze();
		}

		GroupResult group = new(Studio.Library.Root);

		// Run the filtering in another thread.
		await Task.Run(() =>
		{
			group.FilterEntries(filters);
			IEnumerable<Result>? results = group.Get(true);

			if (results == null)
				return;

			this.entries.Clear();
			foreach (Result result in results)
			{
				this.entries.Add(result.Entry);
			}
		});

		await TickService.UiTick();

		if (this.Entries == null)
			this.Entries = new();

		this.Entries.Clear();
		this.Entries.AddRange(this.entries);
	}
}
