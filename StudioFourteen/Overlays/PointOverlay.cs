namespace StudioFourteen.Overlays;

using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

public class PointOverlay
	: OverlayBase
{
	private Vector3 screenPosition;
	private Ellipse? ellipse;

	public Vector3 WorldPosition { get; set; } = Vector3.Zero;

	public override void Update(Canvas canvas)
	{
		this.IsVisible = this.Services.Camera.WorldToCamera(this.WorldPosition, out Vector3 screenPos);
		this.screenPosition = screenPos;

		if (this.ellipse == null)
		{
			this.ellipse = new();
			this.ellipse.Width = 32;
			this.ellipse.Height = 32;
			this.ellipse.Fill = new SolidColorBrush(Colors.Red);
			canvas.Children.Add(this.ellipse);
		}

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
}
