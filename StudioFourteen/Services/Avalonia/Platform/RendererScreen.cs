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

using System;
using System.Collections.Generic;
using global::Avalonia.Platform;
using StudioFourteen.Services.Rendering;

public class StudioScreens : ScreensBase<nint, ScreenImpl>, IDisposable
{
	public readonly ScreenImpl RendererScreen = new(9001);

	private readonly Renderer renderer;

	public StudioScreens(Renderer renderer)
	{
		this.renderer = renderer;
		renderer.ResolutionChanged += this.OnResolutionChanged;
	}

	public void Dispose()
	{
		this.renderer.ResolutionChanged -= this.OnResolutionChanged;
	}

	protected override ScreenImpl CreateScreenFromKey(nint key)
	{
		if (key == this.RendererScreen.Handle)
		{
			return this.RendererScreen;
		}

		throw new NotSupportedException();
	}

	protected override IReadOnlyList<nint> GetAllScreenKeys() => [this.RendererScreen.Handle];

	private void OnResolutionChanged(int width, int height)
	{
		this.RendererScreen?.UpdateSize(width, height);
	}
}
