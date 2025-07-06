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

namespace StudioFourteen.Services;

using StudioFourteen.AIO;
using StudioFourteen.Launcher;
using StudioFourteen.Panels;
using StudioFourteen.Selection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfUtils.Extensions;

using Panel = StudioFourteen.Panels.Panel;
using PanelWindow = StudioFourteen.Panels.PanelWindow;

public class PanelService : ServiceBase
{
	public readonly GamePanelContext GamePanels;
	public readonly AioPanelContext AioPanels;

	private readonly List<PanelContextBase> contexts = new();
	private bool hasRestoredPanels = false;
	private LauncherWindow? launcher;

	public PanelService()
	{
		this.GamePanels = this.CreateContext<GamePanelContext>();
		this.AioPanels = this.CreateContext<AioPanelContext>();
	}

	public delegate void PanelServiceDelegate(PanelService self);

	public event PanelServiceDelegate? PanelsRestarted;

	public string GetPanelTitle(Type type) => Resources.Find($"LOC_{type.Name}", string.Empty);
	public string GetPanelDescription(Type type) => Resources.Find($"LOC_{type.Name}Desc", string.Empty);
	public object? GetPanelIcon(Type type) => Resources.Find($"ICON_Title_{type.Name}");

	public override Task Initialize()
	{
		EventManager.RegisterClassHandler(typeof(FrameworkElement), FrameworkElement.LoadedEvent, new RoutedEventHandler((s, e) => this.OnLoaded(s, e)));
		this.Services.Studio.Opening += this.OnOpening;

		return base.Initialize();
	}

	public override Task Shutdown()
	{
		this.Services.Studio.Opening -= this.OnOpening;
		return base.Shutdown();
	}

	public T CreateContext<T>()
		where T : PanelContextBase, new()
	{
		T context = new T();
		this.contexts.Add(context);
		return context;
	}

	public override async Task Start()
	{
		await base.Start();
		await this.StartPanels();
	}

	public override async Task Stop()
	{
		await base.Stop();
		await this.StopPanels();
	}

	public async Task RestartPanels()
	{
		await this.StopPanels();
		await Task.Delay(1000);
		await this.StartPanels();

		this.PanelsRestarted?.Invoke(this);
	}

	private async Task StartPanels()
	{
		this.launcher = await PanelWindow.CreatePanelWindow<LauncherWindow>(this.GamePanels);
		this.launcher?.Dispatcher.InvokeAsync(() => this.launcher.Show());

		if (!this.hasRestoredPanels && this.Services.Studio.IsOpen)
		{
			this.RestorePanels().Run();
		}
	}

	private async Task StopPanels()
	{
		this.launcher?.Dispatcher.Invoke(this.launcher.Close);

		foreach (PanelContextBase context in this.contexts)
		{
			await context.StopPanels();
		}

		this.hasRestoredPanels = false;
	}

	private async Task RestorePanels()
	{
		if (this.hasRestoredPanels)
			return;

		this.hasRestoredPanels = true;

		// make sure at least one game frame as passed
		await TickService.GameTick();

		// plus a short delay
		await Task.Delay(100);

		foreach (PanelContextBase context in this.contexts)
		{
			await context.RestorePanels();
		}
	}

	private void OnLoaded(object s, RoutedEventArgs e)
	{
		ToolTipService.SetShowOnDisabled((DependencyObject)e.OriginalSource, true);
	}

	private void OnOpening()
	{
		this.GamePanels.CreatePanel<ToolBarPanel>();
		this.GamePanels.CreatePanel<SelectionPanel>();

		if (!this.hasRestoredPanels && this.Services.Studio.IsOpen)
		{
			this.RestorePanels().Run();
		}
	}
}