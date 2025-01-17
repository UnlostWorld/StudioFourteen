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

namespace StudioFourteen.Gizmos;

public class BoxGizmo : GizmoGroup
{
	public BoxGizmo()
	{
		this.Children.Add(new LineGizmo(new(-0.5f, -0.5f, -0.5f), new(0.5f, -0.5f, -0.5f)));
		this.Children.Add(new LineGizmo(new(0.5f, -0.5f, -0.5f), new(0.5f, 0.5f, -0.5f)));
		this.Children.Add(new LineGizmo(new(0.5f, 0.5f, -0.5f), new(-0.5f, 0.5f, -0.5f)));
		this.Children.Add(new LineGizmo(new(-0.5f, 0.5f, -0.5f), new(-0.5f, -0.5f, -0.5f)));

		this.Children.Add(new LineGizmo(new(-0.5f, -0.5f, 0.5f), new(0.5f, -0.5f, 0.5f)));
		this.Children.Add(new LineGizmo(new(0.5f, -0.5f, 0.5f), new(0.5f, 0.5f, 0.5f)));
		this.Children.Add(new LineGizmo(new(0.5f, 0.5f, 0.5f), new(-0.5f, 0.5f, 0.5f)));
		this.Children.Add(new LineGizmo(new(-0.5f, 0.5f, 0.5f), new(-0.5f, -0.5f, 0.5f)));

		this.Children.Add(new LineGizmo(new(-0.5f, -0.5f, -0.5f), new(-0.5f, -0.5f, 0.5f)));
		this.Children.Add(new LineGizmo(new(0.5f, -0.5f, -0.5f), new(0.5f, -0.5f, 0.5f)));
		this.Children.Add(new LineGizmo(new(0.5f, 0.5f, -0.5f), new(0.5f, 0.5f, 0.5f)));
		this.Children.Add(new LineGizmo(new(-0.5f, 0.5f, -0.5f), new(-0.5f, 0.5f, 0.5f)));
	}
}