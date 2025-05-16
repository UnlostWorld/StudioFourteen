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

namespace StudioFourteen.Rendering.Scene.Handles;

using StudioFourteen.Services;
using System.Numerics;
using System.Runtime.CompilerServices;
using StudioFourteen.Rendering.Scene;

public partial class HandleService : ServiceBase
{
	private readonly ConditionalWeakTable<SceneObject, Handle?> parentHandles = new();
	private Handle? currentHover;

	public Handle? CurrentHover
	{
		get => this.currentHover;
		set
		{
			this.currentHover?.OnHover(false);
			this.currentHover = value;
			this.currentHover?.OnHover(true);
		}
	}

	public override void Attach()
	{
		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);
		base.Attach();
	}

	public override void Detach()
	{
		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);
		base.Detach();
	}

	private void OnGameTick()
	{
		Vector2? mousepos = this.Services.Input.Mouse?.GetPosition();

		if (mousepos != null)
		{
			HitTestResult hitTestResult = new();
			this.Services.Rendering.Forward.HitTest(mousepos.Value, hitTestResult);
			this.CurrentHover = this.GetHandle(hitTestResult.SceneObject);
		}
	}

	private Handle? GetHandle(SceneObject? obj)
	{
		if (obj == null)
			return null;

		if (this.parentHandles.TryGetValue(obj, out Handle? handle))
			return handle;

		handle = obj.GetParent<Handle>();
		this.parentHandles.Add(obj, handle);
		return handle;
	}
}