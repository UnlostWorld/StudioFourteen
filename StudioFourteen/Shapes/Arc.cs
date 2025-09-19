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

namespace StudioFourteen.Shapes;

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using DependencyPropertyGenerator;

// adapted from https://stackoverflow.com/questions/23046565/wpf-radial-progressbar-meter-i-e-battery-meter
[DependencyProperty<double>("StartAngle", DefaultValue = 0)]
[DependencyProperty<double>("EndAngle", DefaultValue = 90)]
[DependencyProperty<SweepDirection>("Direction", DefaultValue = SweepDirection.Clockwise)]
[DependencyProperty<double>("OriginRotationDegrees", DefaultValue = -90)]
public partial class Arc : Shape
{
	protected override Geometry DefiningGeometry => this.GetArcGeometry();

	protected override void OnRender(DrawingContext drawingContext)
	{
		drawingContext.DrawGeometry(null, new Pen(this.Stroke, this.StrokeThickness), this.GetArcGeometry());
	}

	partial void OnStartAngleChanged() => this.InvalidateVisual();
	partial void OnEndAngleChanged() => this.InvalidateVisual();
	partial void OnDirectionChanged() => this.InvalidateVisual();
	partial void OnOriginRotationDegreesChanged() => this.InvalidateVisual();

	private Geometry GetArcGeometry()
	{
		Point startPoint = this.PointAtAngle(Math.Min(this.StartAngle, this.EndAngle), this.Direction);
		Point endPoint = this.PointAtAngle(Math.Max(this.StartAngle, this.EndAngle), this.Direction);

		Size arcSize = new Size(
			Math.Max(0, (this.RenderSize.Width - this.StrokeThickness) / 2),
			Math.Max(0, (this.RenderSize.Height - this.StrokeThickness) / 2));
		bool isLargeArc = Math.Abs(this.EndAngle - this.StartAngle) > 180;

		StreamGeometry geom = new StreamGeometry();
		using (StreamGeometryContext context = geom.Open())
		{
			context.BeginFigure(startPoint, false, false);
			context.ArcTo(endPoint, arcSize, 0, isLargeArc, this.Direction, true, false);
		}

		geom.Transform = new TranslateTransform(this.StrokeThickness / 2, this.StrokeThickness / 2);
		return geom;
	}

	private Point PointAtAngle(double angle, SweepDirection sweep)
	{
		double translatedAngle = angle + this.OriginRotationDegrees;
		double radAngle = translatedAngle * (Math.PI / 180);
		double xr = (this.RenderSize.Width - this.StrokeThickness) / 2;
		double yr = (this.RenderSize.Height - this.StrokeThickness) / 2;

		double x = xr + (xr * Math.Cos(radAngle));
		double y = yr * Math.Sin(radAngle);

		if (sweep == SweepDirection.Counterclockwise)
		{
			y = yr - y;
		}
		else
		{
			y = yr + y;
		}

		return new Point(x, y);
	}
}