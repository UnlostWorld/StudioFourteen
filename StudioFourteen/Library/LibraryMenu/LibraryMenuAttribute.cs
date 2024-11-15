namespace StudioFourteen.Library.LibraryMenu;

using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

[AttributeUsage(AttributeTargets.Method)]
public class LibraryMenuAttribute : LibraryMenuAttributeBase
{
	public readonly IconChar? Icon;
	public readonly string? Label;

	public LibraryMenuAttribute(IconChar icon, string label)
	{
		this.Icon = icon;
		this.Label = label;
	}

	public LibraryMenuAttribute(string label)
	{
		this.Label = label;
	}

	public override Task<List<MenuEntry>> GetMenu(LibraryEntryBase entry, MethodInfo method)
	{
		List<MenuEntry> results = new();
		Action invoke = () => method.Invoke(entry, null);
		results.Add(new(this.Icon, this.Label, invoke));
		return Task.FromResult(results);
	}
}
