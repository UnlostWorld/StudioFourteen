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

using System;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Lumina.Excel.Sheets;
using PropertyGenerator.Avalonia;
using StudioFourteen.Services.Library;
using StudioFourteen.Services.Library.GameData.Library;

using ClassJobCategory = StudioFourteen.Services.Library.GameData.Sheets.ClassJobCategory;

public partial class JobsFilter : FilterBase
{
	private ClassJob[]? jobs;

	public JobsFilter()
	{
		this.Jobs = new();
		this.Jobs.CollectionChanged += this.OnJobsChanged;
	}

	[GeneratedStyledProperty]
	public partial AvaloniaList<ClassJob>? Jobs { get; set; }

	[GeneratedStyledProperty]
	public partial bool Healers { get; set; }

	public override void Freeze()
	{
		if (this.Jobs == null || this.Jobs.Count <= 0)
		{
			this.jobs = null;
		}
		else
		{
			this.jobs = this.Jobs.ToArray();
		}
	}

	public override bool Filter(LibraryEntryBase entry)
	{
		if (this.jobs == null)
			return true;

		if (entry is ItemLibraryEntry itemEntry)
		{
			if (itemEntry.ClassJobs == null)
				return false;

			ClassJobCategory classJobs = itemEntry.ClassJobs.Value;

			foreach (ClassJob job in this.jobs)
			{
				if (!classJobs.Contains(job))
				{
					return false;
				}
			}
		}

		return true;
	}

	protected override bool IsFilterProperty(AvaloniaProperty property)
	{
		return property == JobsProperty;
	}

	private void OnJobsChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		this.ApplyFilters();
	}
}