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

using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

public class PointOverlay(string group, string name)
	: OverlayBase(group, name)
{
	private Vector3 screenPosition;
	private Ellipse? ellipse;

	public Vector3 WorldPosition { get; set; } = Vector3.Zero;

	public Color Fill { get; set; } = Colors.White;
	public Color Stroke { get; set; } = Colors.Gray;

	public override void Initialize(Canvas canvas)
	{
		base.Initialize(canvas);

		this.ellipse = new();
		this.ellipse.Width = 8;
		this.ellipse.Height = 8;
		this.ellipse.Fill = new SolidColorBrush(this.Fill);
		this.ellipse.Stroke = new SolidColorBrush(this.Stroke);
		this.ellipse.IsHitTestVisible = false;
		canvas.Children.Add(this.ellipse);
	}

	public override void Update(Canvas canvas)
	{
		if (this.ellipse == null)
			return;

		this.IsVisible = this.Services.Camera.WorldToCamera(this.WorldPosition, out Vector3 screenPos);
		this.screenPosition = screenPos;

		if (this.IsVisible)
		{
			this.ellipse.Visibility = Visibility.Visible;
			Canvas.SetLeft(this.ellipse, (this.screenPosition.X * canvas.ActualWidth) - (this.ellipse.ActualWidth / 2));
			Canvas.SetTop(this.ellipse, (this.screenPosition.Y * canvas.ActualHeight) - (this.ellipse.ActualHeight / 2));
		}
		else
		{
			this.ellipse.Visibility = Visibility.Collapsed;
		}
	}

	public override void Shutdown(Canvas canvas)
	{
		base.Shutdown(canvas);

		canvas.Children.Remove(this.ellipse);
		this.ellipse = null;
	}
}
