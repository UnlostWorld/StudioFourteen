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

namespace StudioFourteen.Services.Xivalonia.Platform;

using System;
using System.Collections.Generic;
using Avalonia.Platform;
using StudioFourteen.Services.Rendering;

public class RendererScreen : ScreensBase<nint, Screen>, IDisposable
{
	// Only one screen in a renderer, so pre create it.
	private readonly Screen screen = new();
	private readonly Renderer renderer;

	public RendererScreen(Renderer renderer)
	{
		this.renderer = renderer;
		renderer.ResolutionChanged += this.OnResolutionChanged;
	}

	public void Dispose()
	{
		this.renderer.ResolutionChanged -= this.OnResolutionChanged;
	}

	protected override Screen CreateScreenFromKey(nint key) => this.screen;
	protected override IReadOnlyList<nint> GetAllScreenKeys() => [0];
	protected override int GetScreenCount() => 1;

	private void OnResolutionChanged(int width, int height)
	{
		this.screen.UpdateSize(width, height);
	}
}
