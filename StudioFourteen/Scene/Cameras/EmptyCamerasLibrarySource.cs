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

namespace StudioFourteen.Scene.Cameras;

using System;
using System.Threading.Tasks;
using StudioFourteen.Library;
using StudioFourteen.Library.Sources;

public class EmptyCamerasLibrarySource : SourceBase
{
	public override string? Name => "Cameras";
	protected override string GetInternalId() => "Cameras";

	protected override void Scan()
	{
		this.Add(new EmptyCamera<OrbitTargetCamera>(this));
		this.Add(new EmptyCamera<OrbitCamera>(this));
		this.Add(new EmptyCamera<FreeCamera>(this));
	}
}

public class EmptyCamera<T>(SourceBase? source)
	: LibraryEntryBase(source), ICameraSave
	where T : Camera, new()
{
	public Type? CameraType => typeof(T);
	public override string? Name => StudioFourteen.Resources.Find($"LOC_{typeof(T).Name}", typeof(T).Name);
	public override string? SubTitle => null;
	public override object? Icon => StudioFourteen.Resources.Find("ICON_Type_Camera");

	public Task Create()
	{
		this.Services.Scene.AddObject<T>($"New {this.Name}");
		return Task.CompletedTask;
	}

	protected override string GetInternalId() => $"EmptyCamera_{typeof(T).Name}";
}