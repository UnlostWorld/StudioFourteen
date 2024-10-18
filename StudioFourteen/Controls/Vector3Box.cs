namespace StudioFourteen.Controls;

using DependencyPropertyGenerator;
using System.Numerics;
using WpfUtils.Controls;

[DependencyProperty<Vector3>("Value")]
public partial class Vector3Box : MultiNumberBox
{
	private bool isValueUpdating = false;
	private bool isComponentUpdating = false;

	protected override void OnComponentValueChanged()
	{
		base.OnComponentValueChanged();

		if (this.isValueUpdating)
			return;

		this.isComponentUpdating = true;

		Vector3 val = this.Value;
		val.X = (float)this.X;
		val.Y = (float)this.Y;
		val.Z = (float)this.Z;
		this.Value = val;

		this.isComponentUpdating = false;
	}

	partial void OnValueChanged(Vector3 newValue)
	{
		if (this.isComponentUpdating)
			return;

		this.isValueUpdating = true;

		this.X = newValue.X;
		this.Y = newValue.Y;
		this.Z = newValue.Z;

		this.isValueUpdating = false;
	}
}
