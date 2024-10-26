namespace StudioFourteen.Controls;

using DependencyPropertyGenerator;
using StudioFourteen.Structs.Extensions;
using System.Numerics;
using WpfUtils.Controls;

[DependencyProperty<Quaternion>("Value", DefaultBindingMode = DefaultBindingMode.TwoWay)]
public partial class QuaternionBox : MultiNumberBox
{
	private bool isValueUpdating = false;
	private bool isComponentUpdating = false;

	protected override void OnComponentValueChanged()
	{
		base.OnComponentValueChanged();

		if (this.isValueUpdating)
			return;

		this.isComponentUpdating = true;

		Vector3 euler = this.Value.ToEuler();
		euler.X = (float)this.X;
		euler.Y = (float)this.Y;
		euler.Z = (float)this.Z;

		Quaternion val = this.Value;
		val.FromEuler(euler);
		this.Value = val;

		this.isComponentUpdating = false;
	}

	partial void OnValueChanged(Quaternion newValue)
	{
		if (this.isComponentUpdating)
			return;

		this.isValueUpdating = true;

		Vector3 euler = newValue.ToEuler();
		this.X = euler.X;
		this.Y = euler.Y;
		this.Z = euler.Z;

		this.isValueUpdating = false;
	}
}
