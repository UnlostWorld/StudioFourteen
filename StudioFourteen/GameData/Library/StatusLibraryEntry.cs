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

namespace StudioFourteen.GameData.Library;

using Lumina.Excel.Sheets;
using StudioFourteen.Library.Sources;

public class StatusLibraryEntry : ExcelLibraryEntry
{
	public readonly Status Status;

	public StatusLibraryEntry(SourceBase source, Status status)
		: base(source, status.RowId)
	{
		this.Status = status;
		if (status.CanDispel)
		{
			this.Tags.Add("Can Esuna").WithAlias("Can Dispel");
		}

		if (this.Status.StatusCategory == 1)
		{
			this.Tags.Add("Enhancement");
		}
		else if (this.Status.StatusCategory == 2)
		{
			this.Tags.Add("Enfeeblement");
		}
		else
		{
			this.Tags.Add("Status Uncategorized");
		}

		if (this.Status.Unknown0 == 170)
		{
			this.Tags.Add("PVP Action");
		}

		this.Tags.Add($"U0: {this.Status.Unknown0}");
	}

// Unknown0: Represents type of status Effect / Mechanic
// Unknown2: Appears to categorise different types of DOT
// Unknown3: Appears to include statuses where movement is prevented or affected by another source
// Unknown5: hmm more dots?
// Unknown6: idk
// Unknown7: True for skill #3673
// Unknown_70_1: Appears unused
// Unknown_70_2: True for 3 skills
	public override string? Name => this.Status.Name.ToString();
	public override object? Icon => new ImageReference(this.Status.Icon);
	public object? Description => $"{this.Status.Description} \n {this.Status.StatusCategory}\n" +
	$"Unknown0 {this.Status.Unknown0}\n Unknown2 {this.Status.Unknown2}\n Unknown3 {this.Status.Unknown3}\n" +
	$"Unknown5 {this.Status.Unknown5}\n Unknown6 {this.Status.Unknown6}\n Unknown7 {this.Status.Unknown7}\n" +
	$"Unknown_70_1 {this.Status.Unknown_70_1}\n Unknown_70_2 {this.Status.Unknown_70_2}\n" +
	$"Esuna {this.Status.CanDispel}\n";
}