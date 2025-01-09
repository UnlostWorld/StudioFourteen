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
using StudioFourteen.Overlays.Primitives;
using System.Numerics;

public class TestOverlayLayer : OverlayLayerBase
{
	private readonly LinePrimitive line;
	private readonly CirclePrimitive circle;

	public TestOverlayLayer()
		: base("Test", "Layer 1")
	{
		this.line = new LinePrimitive()
		{
			From = new(0, 0, 0),
			To = new(0, 100, 0),
		};

		this.AddChild(this.line);

		this.circle = new CirclePrimitive()
		{
			Radius = 1.0f,
		};

		this.AddChild(this.circle);
	}

	public unsafe override void OnFrameworkUpdate()
	{
		base.OnFrameworkUpdate();

		Character* target = this.Services.Target.GetTarget();
		this.line.Transform = target->GameObject.GetTransform();
		this.line.To.Y = target->Height;

		this.circle.Transform = target->GameObject.GetTransform();
		this.circle.Radius = target->HitboxRadius;
	}
}
