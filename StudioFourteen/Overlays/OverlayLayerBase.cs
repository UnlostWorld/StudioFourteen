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

namespace StudioFourteen.Overlays;

using Serilog;
using StudioFourteen.Gizmos;
using StudioFourteen.Settings;

public abstract class OverlayLayerBase(string group, string name)
	: GizmoGroup
{
	public readonly string Group = group;
	public readonly string Name = name;

	protected readonly Persistence persistence = new($"Overlay_{group}_{name}");

	public string DisplayGroup => Resources.Find($"LOC_OverlayGroup_{this.Group}", this.Group);
	public string DisplayName => Resources.Find($"LOC_Overlay_{this.Name}", this.Name);

	public bool IsHidden
	{
		get => this.persistence.GetPersistence<bool>();
		set
		{
			this.persistence.SetPersistence(value);
			this.IsVisible = !value;
		}
	}

	public void Enable()
	{
		this.Services.Overlays.AddOverlay(this);
		this.IsVisible = !this.IsHidden;
	}

	public void Disable()
	{
		this.Services.Overlays.RemoveOverlay(this);
	}

	public virtual void OnTick()
	{
	}
}
