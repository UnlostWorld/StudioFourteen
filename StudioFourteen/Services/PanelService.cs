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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using StudioFourteen.AIO;
using StudioFourteen.Launcher;
using StudioFourteen.Panels;
using StudioFourteen.Selection;
using StudioFourteen.Settings;
using StudioFourteen.Xaml;
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

	public string GetPanelTitle(Type type) => XamlResources.Find($"LOC_{type.Name}", string.Empty);
	public string GetPanelDescription(Type type) => XamlResources.Find($"LOC_{type.Name}Desc", string.Empty);
	public object? GetPanelIcon(Type type) => XamlResources.Find($"ICON_Title_{type.Name}");

	public override Task Initialize()
	{
		EventManager.RegisterClassHandler(typeof(FrameworkElement), FrameworkElement.LoadedEvent, new RoutedEventHandler((s, e) => this.OnLoaded(s, e)));
		this.Services.Studio.Opening += this.OnOpening;
		this.Services.Studio.Closing += this.OnClosing;
		this.Services.Settings.SettingChanged += this.OnSettingChanged;

		return base.Initialize();
	}

	public override Task Shutdown()
	{
		this.Services.Studio.Opening -= this.OnOpening;
		this.Services.Studio.Closing -= this.OnClosing;
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
		try
		{
			await this.StopPanels();
			await Task.Delay(1000);
			await this.StartPanels();
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error restarting panels");
			await this.StartPanels();
		}

		this.PanelsRestarted?.Invoke(this);
	}

	private async Task StartPanels()
	{
		this.launcher = await PanelWindow.CreatePanelWindow<LauncherWindow>(this.GamePanels, "Launcher window");
		this.launcher?.Dispatcher.InvokeAsync(() => this.launcher.Show());

		if (!this.hasRestoredPanels && this.Services.Studio.IsOpen)
		{
			this.RestorePanels().RunAsynchronously();
		}
	}

	private async Task StopPanels()
	{
		try
		{
			this.launcher?.Close();
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error closing launcher");
		}

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

	private void OnSettingChanged(string settingName, object? newValue)
	{
		if (settingName == nameof(this.Settings.EnableTargetBar))
		{
			this.CheckTargetBar();
		}
		else if (settingName == nameof(this.Settings.AllInOne))
		{
			this.CheckTargetBar();
			this.CheckToolBar();
		}
	}

	private void OnOpening()
	{
		this.CheckToolBar();
		this.CheckTargetBar();

		if (!this.hasRestoredPanels && this.Services.Studio.IsOpen)
		{
			this.RestorePanels().RunAsynchronously();
		}
	}

	private void OnClosing()
	{
		this.GamePanels.SetIsOpen<SelectionPanel>(false, false);
		this.GamePanels.SetIsOpen<ToolBarPanel>(false, false);
	}

	private void CheckTargetBar()
	{
		bool enableTargetBar = this.Settings.EnableTargetBar;
		enableTargetBar &= this.Settings.AllInOne != Configuration.AioModes.Always;
		this.GamePanels.SetIsOpen<SelectionPanel>(enableTargetBar, false);
	}

	private void CheckToolBar()
	{
		this.GamePanels.SetIsOpen<ToolBarPanel>(true, false);
	}
}