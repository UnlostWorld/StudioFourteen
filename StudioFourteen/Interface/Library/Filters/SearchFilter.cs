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

using System.Windows.Input;
using Avalonia;
using CommunityToolkit.Mvvm.Input;
using PropertyGenerator.Avalonia;
using StudioFourteen.Services.Library;

public partial class SearchFilter : FilterBase
{
	private string[]? query;

	public SearchFilter()
	{
		this.ClearCommand = new RelayCommand(this.Clear);
	}

	[GeneratedStyledProperty]
	public partial string Search { get; set; }

	[GeneratedStyledProperty]
	public partial ICommand ClearCommand { get; set; }

	public void Clear()
	{
		this.Search = string.Empty;
	}

	public override void Freeze()
	{
		if (string.IsNullOrEmpty(this.Search))
		{
			this.query = null;
		}
		else
		{
			this.query = SearchUtility.ToQuery(this.Search);
		}
	}

	public override bool Filter(LibraryEntryBase entry)
	{
		if (this.query == null)
			return true;

		return entry.Search(this.query);
	}

	protected override bool IsFilterProperty(AvaloniaProperty property)
	{
		return property == SearchProperty;
	}
}