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

namespace StudioFourteen.AIO;

using StudioFourteen.Panels;
using System.Threading.Tasks;
using System.Windows;
using DependencyPropertyGenerator;
using System;
using WpfUtils;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Environment;
using StudioFourteen.Selection;
using StudioFourteen.Launcher;
using StudioFourteen.Posing;
using StudioFourteen.Animation;

public partial class AioWindow : PanelWindow
{
	private static AioWindow? instance;

	[Notify] private Panel? currentPanel;
	[Notify] private string? currentTitle;
	[Notify] private bool showTargetBar = false;
	[Notify] private bool isStudioAttached = false;

	public static void OpenAio()
	{
		if (instance != null)
		{
			instance.Dispatcher?.Invoke(instance.Activate);
			return;
		}

		Task.Run(async () =>
		{
			AioWindow? aio = await PanelWindow.CreatePanelWindow<AioWindow>(ServiceManager.Instance.Panels.AioPanels, "Aio Window");
			if (aio != null)
			{
				ServiceManager.Instance.Panels.AioPanels.Window = aio;
				instance = aio;

				aio.Dispatcher.Invoke(() =>
				{
					aio.Show();
				});
			}
		});
	}

	public static void CloseAio()
	{
		if (instance == null)
			return;

		instance.Dispatcher?.BeginInvoke(() => instance.Close());
	}

	public static bool GetIsOpen()
	{
		if (instance == null)
			return false;

		return true;
	}

	public async Task<Panel?> CreatePanel(Type panelType)
	{
		await this.MainThread();
		this.CurrentPanel = this.PanelArea.SetPanel(panelType);

		string currentTitle = StudioFourteen.Resources.Find("LOC_AIO_Title", "Studio Fourteen");
		if (this.CurrentPanel != null)
			currentTitle += " - " + this.Services.Panels.GetPanelTitle(this.CurrentPanel.GetType());

		this.ShowTargetBar = this.currentPanel is PosePanel || this.currentPanel is AnimationPanel;
		this.CurrentTitle = currentTitle;

		return this.CurrentPanel;
	}

	protected override bool GetIsUiVisibleAndOpen() => true;
	protected override bool GetIsUiVisible() => true;

	protected override void OnOpened()
	{
		this.CurrentTitle = StudioFourteen.Resources.Find("LOC_AIO_Title", "Studio Fourteen");
		this.IsStudioAttached = this.Services.Studio.IsAttached;

		this.Services.Studio.Opening += this.OnStudioOpening;
		this.Services.Studio.Closing += this.OnStudioClosing;

		base.OnOpened();

		Task.Run(async () =>
		{
			await Task.Delay(250);
			await this.MainThread();

			this.TaskBar.AddEntry<Marketplace.MarketplacePanel>();
			this.TaskBar.AddEntry<Library.LibraryPanel>();
			this.TaskBar.AddEntry<InspectorPanel>();
			this.TaskBar.AddEntry<EnvironmentPanel>();
			this.TaskBar.AddEntry<Posing.PosePanel>();
			this.TaskBar.AddEntry<Animation.AnimationPanel>();
			this.TaskBar.AddEntry<Photos.PhotoPanel>();
			this.TaskBar.AddEntry<Settings.SettingsPanel>();
		});
	}

	protected override void OnClosed()
	{
		this.Services.Studio.Opening -= this.OnStudioOpening;
		this.Services.Studio.Closing -= this.OnStudioClosing;

		base.OnClosed();
		instance = null;
	}

	private void OnStudioOpening()
	{
		this.IsStudioAttached = true;
	}

	private void OnStudioClosing()
	{
		this.IsStudioAttached = false;
	}

	private void OnIsStudioAttachedChanged(bool oldValue, bool newValue)
	{
		if (newValue)
		{
			this.Services.Studio.OpenStudio();
		}
		else
		{
			this.Services.Studio.CloseStudio();
			this.CurrentPanel = this.PanelArea.SetPanel(null);
			this.CurrentTitle = StudioFourteen.Resources.Find("LOC_AIO_Title", "Studio Fourteen");
		}
	}
}