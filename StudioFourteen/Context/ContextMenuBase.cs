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

namespace StudioFourteen.Context;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfUtils.Commands;
using WpfUtils.Extensions;

public interface IContextMenu
{
	void OnMenuInvoked(MenuEntry entry);
	Task OnMenusLoaded(List<MenuEntry> entries);
}

public class MenuEntry
{
	private readonly Func<Task>? invoke;

	public MenuEntry()
	{
	}

	public MenuEntry(object? icon, string label, Func<Task>? invoke = null)
	{
		if (icon is string str)
			icon = Resources.Find(str);

		this.Icon = icon;
		this.Label = Resources.Find(label, label);
		this.invoke = invoke;
		this.OnClicked = new SimpleCommand(this.Invoke);
	}

	public object? Icon { get; set; }
	public string? Label { get; set; }
	public ICommand? OnClicked { get; set; }

	public bool IsEnabled { get; set; } = true;

	public FastObservableCollection<MenuEntry> Children { get; init; } = new();
	public IContextMenu? ContextMenu { get; set; }

	public bool HasChildren => this.Children.Count > 0;

	public Task Invoke()
	{
		Task? t = this.invoke?.Invoke();
		this.ContextMenu?.OnMenuInvoked(this);

		if (t == null)
			return Task.CompletedTask;

		return t;
	}

	public void AddChild(MenuEntry child)
	{
		this.Children.Add(child);
	}
}