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
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls.Primitives;
using CommunityToolkit.Mvvm.Input;
using PropertyGenerator.Avalonia;
using StudioFourteen.Services.Library;
using StudioFourteen.Services.Library.Filters;
using StudioFourteen.Services.Library.Results;
using StudioFourteen.Services.Tick;

public partial class LibraryInspector : TemplatedControl
{
	private readonly List<LibraryEntryBase> entries = new();

	[GeneratedStyledProperty]
	public partial LibraryEntryBase? Value { get; set; }

	[GeneratedStyledProperty]
	public partial AvaloniaList<LibraryEntryBase> Entries { get; set; }

	[GeneratedStyledProperty]
	public partial string Filters { get; set; }

	[RelayCommand]
	public void Search()
	{
		string filters = this.Filters;
		Task.Run(async () => await this.SearchAsyncSafe(filters));
	}

	partial void OnFiltersPropertyChanged(string newValue)
	{
		this.Search();
	}

	private async Task SearchAsyncSafe(string filters)
	{
		try
		{
			await this.SearchAsync(filters);
		}
		catch (Exception ex)
		{
			Studio.Log.Error(ex, "Error searching library");
		}
	}

	private async Task SearchAsync(string filterInputs)
	{
		List<FilterBase> filters = Studio.Library.GetFilters(filterInputs);

		GroupResult group = new(Studio.Library.Root);
		group.FilterEntries(filters.ToArray());
		IEnumerable<Result>? results = group.Get(true);

		if (results == null)
			return;

		this.entries.Clear();
		foreach (Result result in results)
			this.entries.Add(result.Entry);

		await TickService.UiTick();

		if (this.Entries == null)
			this.Entries = new();

		this.Entries.Clear();
		this.Entries.AddRange(this.entries);
	}
}