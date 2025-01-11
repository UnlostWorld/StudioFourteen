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

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using StudioFourteen.Overlays.Gizmos.Rotation;
using StudioFourteen.Overlays.Gizmos.Translation;

public class TestOverlayLayer : OverlayLayerBase
{
	private readonly TranslationGizmo gizmo;
	private Posing.Transform? pendingTransform;

	public TestOverlayLayer()
		: base("Test", "Layer 1")
	{
		this.gizmo = new()
		{
			Sensitivity = 1,
		};

		this.gizmo.TransformChanged += this.OnTransformChanged;

		this.AddChild(this.gizmo);
	}

	public unsafe override void OnFrameworkUpdate()
	{
		base.OnFrameworkUpdate();

		Character* target = this.Services.Target.GetTarget();

		if (this.pendingTransform != null)
		{
			target->GameObject.SetTransform(this.pendingTransform.Value);
			this.pendingTransform = null;
		}

		this.gizmo.Transform = target->GameObject.GetTransform();
	}

	private void OnTransformChanged(Posing.Transform newTransform)
	{
		this.pendingTransform = newTransform;
	}
}
