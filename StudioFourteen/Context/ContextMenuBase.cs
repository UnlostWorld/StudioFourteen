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
}

public class MenuEntry
{
	private readonly List<MenuEntry> pendingChildren = new();
	private readonly Func<Task>? invoke;

	public MenuEntry()
	{
	}

	public MenuEntry(object? icon, string? label, Func<Task>? invoke = null)
	{
		if (icon is string str)
			icon = Resources.Find(str);

		this.Icon = icon;
		this.Label = label;
		this.invoke = invoke;
		this.OnClicked = new SimpleCommand(this.Invoke);
	}

	public object? Icon { get; set; }
	public string? Label { get; set; }
	public ICommand? OnClicked { get; set; }

	public bool IsEnabled { get; set; } = true;

	public FastObservableCollection<MenuEntry> Children { get; init; } = new();
	public IContextMenu? ContextMenu { get; private set; }

	public bool HasChildren => this.Children.Count > 0 || this.pendingChildren.Count > 0;

	public void SetContextMenu(IContextMenu? contextMenu)
	{
		this.ContextMenu = contextMenu;

		foreach (MenuEntry entry in this.pendingChildren)
		{
			this.Children.Add(entry);
		}

		this.pendingChildren.Clear();

		foreach (MenuEntry entry in this.Children)
		{
			entry.SetContextMenu(contextMenu);
		}
	}

	public Task Invoke()
	{
		Task? t = this.invoke?.Invoke();
		this.ContextMenu?.OnMenuInvoked(this);

		if (t == null)
			return Task.CompletedTask;

		return t;
	}

	public MenuEntry AddChild(object? icon, string? label, Func<Task>? invoke = null)
	{
		MenuEntry child = new(icon, label, invoke);
		this.pendingChildren.Add(child);
		return child;
	}

	public MenuEntry AddChild(object? icon, string? label, Action? invoke = null)
	{
		Func<Task> f = () =>
		{
			invoke?.Invoke();
			return Task.CompletedTask;
		};

		MenuEntry child = new(icon, label, f);
		this.pendingChildren.Add(child);
		return child;
	}
}