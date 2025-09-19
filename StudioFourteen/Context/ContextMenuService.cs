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
using Serilog;
using StudioFourteen.Appearance;
using StudioFourteen.Environment;
using StudioFourteen.Library.Sources;
using StudioFourteen.Services;

public interface IContextProvider
{
	Type GetTargetType();
	Task GetMenus(object target, List<MenuEntry> menus);
}

public abstract class ContextProvider<T> : IContextProvider
{
	protected readonly ILogger Log;

	public ContextProvider()
	{
		this.Log = Logging.ForContext(this.GetType());
	}

	protected ServiceManager Services => ServiceManager.Instance;

	public Task GetMenus(object target, List<MenuEntry> menus)
	{
		if (target is not T tTarget)
			return Task.CompletedTask;

		return this.GetMenus(tTarget, menus);
	}

	public Type GetTargetType() => typeof(T);

	protected abstract Task GetMenus(T target, List<MenuEntry> menus);
}

[Service]
public class ContextMenuService : ServiceBase
{
	private readonly List<IContextProvider> providers = new();

	public override Task Start()
	{
		this.RegisterProvider<CharacterAppearanceContextMenuProvider>();
		this.RegisterProvider<EnvironmentFileContextMenuProvider>();
		this.RegisterProvider<SkyTextureContextMenuProvider>();

		return base.Start();
	}

	public void RegisterProvider<T>()
		where T : IContextProvider, new()
	{
		this.RegisterProvider(new T());
	}

	public void RegisterProvider(IContextProvider provider)
	{
		this.providers.Add(provider);
	}

	public void GetContext(IContextMenu menu, params object[] targets)
	{
		this.GetContextAsync(menu, targets).RunAsynchronously();
	}

	public async Task GetContextAsync(IContextMenu menu, params object[] targets)
	{
		List<MenuEntry> menus = new();
		foreach (object target in targets)
		{
			object? targetActual = target;
			if (targetActual is FileEntry fileEntry)
				targetActual = fileEntry.File;

			if (targetActual == null)
				continue;

			Type objectType = targetActual.GetType();

			foreach (IContextProvider provider in this.providers)
			{
				if (!objectType.IsAssignableTo(provider.GetTargetType()))
					continue;

				await provider.GetMenus(targetActual, menus);
			}
		}

		foreach (MenuEntry entry in menus)
		{
			entry.ContextMenu = menu;
		}

		await menu.OnMenusLoaded(menus);
	}
}