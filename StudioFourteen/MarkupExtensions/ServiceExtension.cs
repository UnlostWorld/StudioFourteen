namespace StudioFourteen.MarkupExtensions;

using System;
using System.Windows.Markup;

public class ServicesExtension
	: MarkupExtension
{
	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		return ServiceManager.Instance;
	}
}