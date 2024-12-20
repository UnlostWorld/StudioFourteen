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

namespace StudioFourteen.Library;

using FontAwesome.Sharp;
using StudioFourteen.Library.LibraryMenu;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LibraryDoubleClickContext : ILibraryContextMenu
{
	private readonly List<MenuEntry> menus = new();

	public async Task Execute(LibraryEntryBase entry)
	{
		this.menus.Clear();
		await entry.GetLibraryMenus(this);
		foreach (MenuEntry menu in this.menus)
		{
			if (menu.HasChildren)
				continue;

			if (!menu.IsEnabled)
				continue;

			await menu.Invoke();
			break;
		}
	}

	public MenuEntry AddMenu(IconChar? icon, string? label, Func<Task>? invoke = null)
	{
		MenuEntry entry = new(icon, label, invoke);
		this.menus.Add(entry);
		return entry;
	}

	public MenuEntry AddMenu(IconChar? icon, string? label)
	{
		MenuEntry entry = new(icon, label);
		this.menus.Add(entry);
		return entry;
	}

	public MenuEntry AddMenu(IconChar? icon, string? label, Action invoke)
	{
		Func<Task> f = () =>
		{
			invoke?.Invoke();
			return Task.CompletedTask;
		};

		MenuEntry entry = new(icon, label, f);
		this.menus.Add(entry);
		return entry;
	}
}
