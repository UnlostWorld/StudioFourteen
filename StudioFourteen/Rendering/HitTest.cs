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

namespace StudioFourteen.Rendering;

using System.Numerics;
using StudioFourteen.Rendering.Scene;

public class HitTestResult
{
	public float Distance = float.MaxValue;
	public float MaxDistance = float.MaxValue;
	public Vertex? MeshVertex;
	public SceneObject? SceneObject;
	public bool Handled = false;
	public float Depth = -1;
	public Vector2 ScreenNormal = Vector2.Zero;

	public void Clear()
	{
		this.Distance = float.MaxValue;
		this.MaxDistance = float.MaxValue;
		this.MeshVertex = null;
		this.SceneObject = null;
		this.Handled = false;
		this.Depth = -1;
		this.ScreenNormal = Vector2.Zero;
	}
}