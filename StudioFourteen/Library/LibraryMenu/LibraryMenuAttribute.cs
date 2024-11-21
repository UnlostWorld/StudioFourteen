// .                      @@             _____ _______ _    _ _____ _____ ____
//            @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//           @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//           @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//          @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//      @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//       @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//        @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//        @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//      @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//       @@@@             @@@
//         @@@@@      @@@@@               This software is licensed under the
//          @@@@@@@@@@@@@@                 GNU AFFERO GENERAL PUBLIC LICENSE
//              @@@@  @                       Version 3, 19 November 2007

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
		this.Label = StudioFourteen.Resources.Find(label, label);
	}

	public LibraryMenuAttribute(string label)
	{
		this.Label = StudioFourteen.Resources.Find(label, label);
	}

	public override Task<List<MenuEntry>> GetMenu(object methodTarget, MethodInfo method)
	{
		List<MenuEntry> results = new();
		Action invoke = () => method.Invoke(methodTarget, null);
		results.Add(new(this.Icon, this.Label, invoke));
		return Task.FromResult(results);
	}
}
