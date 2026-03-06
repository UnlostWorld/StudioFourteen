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

using System.Collections.Specialized;
using Avalonia.Collections;
using Avalonia.Controls.Primitives;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using PropertyGenerator.Avalonia;

public partial class RoleControl : TemplatedControl
{
	private bool supressIncludeChanges = false;

	[GeneratedStyledProperty]
	public partial int Id { get; set; }

	[GeneratedStyledProperty]
	public partial bool Include { get; set; }

	[GeneratedStyledProperty]
	public partial AvaloniaList<ClassJob>? Jobs { get; set; }

	[GeneratedStyledProperty]
	public partial int IconId { get; set; }

	partial void OnJobsPropertyChanged(AvaloniaList<ClassJob>? oldValue, AvaloniaList<ClassJob>? newValue)
	{
		oldValue?.CollectionChanged -= this.OnJobsChanged;
		newValue?.CollectionChanged += this.OnJobsChanged;
	}

	private void OnJobsChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		this.supressIncludeChanges = true;
		if (this.Jobs == null || this.Jobs.Count == 0)
		{
			this.Include = false;
		}
		else
		{
			bool hasAllOfRole = true;
			foreach (ClassJob classJob in Studio.DataManager.GetExcelSheet<ClassJob>())
			{
				if (this.Id == 5)
				{
					if (classJob.ClassJobCategory.RowId != 33 && classJob.ClassJobCategory.RowId != 32)
						continue;
				}
				else
				{
					if (classJob.Role != this.Id)
						continue;
				}

				hasAllOfRole &= this.Jobs.Contains(classJob);
			}

			this.Include = hasAllOfRole;
		}

		this.supressIncludeChanges = false;
	}

	partial void OnIdPropertyChanged(int newValue)
	{
		this.IconId = newValue switch
		{
			1 => 062581, // Tanks
			2 => 062584, // Melee
			3 => 062585, // Ranged
			4 => 062582, // Healer
			5 => 061816, // Crafter / Gatherer
			_ => 0,
		};
	}

	partial void OnIncludePropertyChanged(bool newValue)
	{
		if (this.Jobs == null)
			return;

		if (this.supressIncludeChanges)
			return;

		foreach (ClassJob classJob in Studio.DataManager.GetExcelSheet<ClassJob>())
		{
			if (this.Id == 5)
			{
				if (classJob.ClassJobCategory.RowId != 33 && classJob.ClassJobCategory.RowId != 32)
					continue;
			}
			else
			{
				if (classJob.Role != this.Id)
					continue;
			}

			if (newValue && !this.Jobs.Contains(classJob))
			{
				this.Jobs.Add(classJob);
			}
			else if (!newValue && this.Jobs.Contains(classJob))
			{
				this.Jobs.Remove(classJob);
			}
		}
	}
}