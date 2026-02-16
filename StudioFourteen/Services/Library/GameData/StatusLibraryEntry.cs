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

namespace StudioFourteen.Services.Library.GameData.Library;

using Lumina.Excel.Sheets;

public class StatusLibraryEntry : ExcelLibraryEntry
{
	public readonly Status Status;

	public StatusLibraryEntry(SourceBase source, Status status)
		: base(source, status.RowId)
	{
		this.Status = status;
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
	public override object? Icon => new TextureIconReference(this.Status.Icon);
	public object? Description => $"{this.Status.Description}";
}