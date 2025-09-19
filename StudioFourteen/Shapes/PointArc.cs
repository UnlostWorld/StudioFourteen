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

using DependencyPropertyGenerator;

[DependencyProperty<double>("Minimum")]
[DependencyProperty<double>("Maximum")]
[DependencyProperty<double>("Value")]
[DependencyProperty<double>("Spread", DefaultValue = 12)]
[DependencyProperty<double>("AngleOffset", DefaultValue = 0)]
public partial class PointArc : Arc
{
	partial void OnMinimumChanged() => this.UpdateArc();
	partial void OnMaximumChanged() => this.UpdateArc();
	partial void OnValueChanged() => this.UpdateArc();
	partial void OnAngleOffsetChanged() => this.UpdateArc();
	partial void OnSpreadChanged() => this.UpdateArc();

	private void UpdateArc()
	{
		double range = this.Maximum - this.Minimum;
		if (range == 0)
			return;

		double p = this.Value / range;
		double angle = this.AngleOffset + (359.999 * p);

		this.EndAngle = angle - (this.Spread / 2);
		this.StartAngle = angle + (this.Spread / 2);
	}
}
