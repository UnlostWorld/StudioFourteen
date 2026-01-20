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

namespace StudioFourteen.Services.Avalonia.Platform;

using global::Avalonia.Platform;

public class ScreenImpl : PlatformScreen
{
	public readonly nint Handle;

	public ScreenImpl(nint handle)
		: base(new PlatformHandle(handle, "Xiv Screen"))
	{
		this.Handle = handle;
		this.DisplayName = "Xiv Display";
		this.CurrentOrientation = ScreenOrientation.Landscape;
		this.IsPrimary = true;
		this.Scaling = 1.0;

		this.UpdateSize(1920, 1080);
	}

	public void UpdateSize(int width, int height)
	{
		this.WorkingArea = new global::Avalonia.PixelRect(0, 0, width, height);
		this.Bounds = new global::Avalonia.PixelRect(0, 0, width, height);
	}
}