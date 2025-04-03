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

namespace StudioFourteen.Controls;

using DependencyPropertyGenerator;
using StudioFourteen.Structs.Extensions;
using System.Numerics;

[DependencyProperty<Quaternion>("Quaternion", DefaultBindingMode = DefaultBindingMode.TwoWay)]
public partial class QuaternionBox : Vector3Control
{
	private bool isValueUpdating = false;
	private bool isComponentUpdating = false;

	public QuaternionBox()
	{
		this.Minimum = -180;
		this.Maximum = 180;
		this.Wrap = true;
		this.Change = 5;
		this.DecimalPlaces = 2;
	}

	protected override void OnInternalValueChanged(Vector3 internalValue)
	{
		base.OnInternalValueChanged(internalValue);

		if (this.isValueUpdating)
			return;

		this.isComponentUpdating = true;

		Vector3 euler = this.Quaternion.ToEuler();
		euler.X = (float)this.ValueX;
		euler.Y = (float)this.ValueY;
		euler.Z = (float)this.ValueZ;

		Quaternion val = this.Quaternion;
		val.FromEuler(euler);
		this.Quaternion = val;

		this.isComponentUpdating = false;
	}

	partial void OnQuaternionChanged(Quaternion newValue)
	{
		if (this.isComponentUpdating)
			return;

		this.isValueUpdating = true;

		Vector3 euler = newValue.ToEuler();
		this.Value = euler;

		this.isValueUpdating = false;
	}
}
