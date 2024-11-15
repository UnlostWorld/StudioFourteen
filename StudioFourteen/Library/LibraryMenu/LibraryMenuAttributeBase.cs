namespace StudioFourteen.Library.LibraryMenu;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

[AttributeUsage(AttributeTargets.Method)]
public abstract class LibraryMenuAttributeBase : Attribute
{
	public abstract Task<List<MenuEntry>> GetMenu(object methodTarget, MethodInfo method);
}
