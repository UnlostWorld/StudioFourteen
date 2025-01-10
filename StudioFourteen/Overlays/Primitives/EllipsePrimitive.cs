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

namespace StudioFourteen.Overlays.Primitives;

using System;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

public class EllipsePrimitive : PrimitiveBase
{
	public Color Foreground = Colors.White;
	public float Radius = 100;

	protected Ellipse? ellipse;

	public override void Enable(Canvas canvas)
	{
		this.ellipse = new();
		this.ellipse.Fill = new SolidColorBrush(this.Foreground);
		this.ellipse.IsHitTestVisible = false;
		canvas.Children.Add(this.ellipse);
		base.Enable(canvas);
	}

	public override void Disable(Canvas canvas)
	{
		base.Disable(canvas);

		canvas.Children.Remove(this.ellipse);
		this.ellipse = null;
	}

	public override void Update()
	{
		if (this.ellipse == null)
			return;

		Vector3 centerPos = this.LocalToScreen(Vector3.Zero);

		Canvas.SetLeft(this.ellipse, centerPos.X - this.Radius);
		Canvas.SetTop(this.ellipse, centerPos.Y - this.Radius);
		this.ellipse.Width = this.Radius * 2;
		this.ellipse.Height = this.Radius * 2;

		this.SetZIndex(this.ellipse, centerPos.Z);
	}
}