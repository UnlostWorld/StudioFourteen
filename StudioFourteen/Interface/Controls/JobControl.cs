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

public partial class JobControl : TemplatedControl
{
	private bool supressIncludeChanges = false;

	[GeneratedStyledProperty]
	public partial int Id { get; set; }

	[GeneratedStyledProperty]
	public partial ClassJob ClassJob { get; set; }

	[GeneratedStyledProperty]
	public partial bool Include { get; set; }

	[GeneratedStyledProperty]
	public partial AvaloniaList<ClassJob>? Jobs { get; set; }

	partial void OnIdPropertyChanged(int newValue)
	{
		this.ClassJob = Studio.DataManager.GetRow<ClassJob>((uint)newValue);
	}

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
			this.Include = this.Jobs.Contains(this.ClassJob);
		}

		this.supressIncludeChanges = false;
	}

	partial void OnIncludePropertyChanged(bool newValue)
	{
		if (this.Jobs == null)
			return;

		if (this.supressIncludeChanges)
			return;

		if (newValue && !this.Jobs.Contains(this.ClassJob))
		{
			this.Jobs.Add(this.ClassJob);
		}
		else if (!newValue && this.Jobs.Contains(this.ClassJob))
		{
			this.Jobs.Remove(this.ClassJob);
		}
	}
}