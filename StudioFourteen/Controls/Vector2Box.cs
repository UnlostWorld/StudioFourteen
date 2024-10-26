namespace StudioFourteen.Controls;

using DependencyPropertyGenerator;
using System.Numerics;
using WpfUtils.Controls;

[DependencyProperty<Vector2>("Value", DefaultBindingMode = DefaultBindingMode.TwoWay)]
public partial class Vector2Box : MultiNumberBox
{
	private bool isValueUpdating = false;
	private bool isComponentUpdating = false;

	protected override void OnComponentValueChanged()
	{
		base.OnComponentValueChanged();

		if (this.isValueUpdating)
			return;

		this.isComponentUpdating = true;

		Vector2 val = this.Value;
		val.X = (float)this.X;
		val.Y = (float)this.Y;
		this.Value = val;

		this.isComponentUpdating = false;
	}

	partial void OnValueChanged(Vector2 newValue)
	{
		if (this.isComponentUpdating)
			return;

		this.isValueUpdating = true;

		this.X = newValue.X;
		this.Y = newValue.Y;

		this.isValueUpdating = false;
	}
}
