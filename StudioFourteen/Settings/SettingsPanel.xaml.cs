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

namespace StudioFourteen.Settings;

using StudioFourteen.Panels;
using StudioFourteen.ResourcePacks;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfUtils;
using WpfUtils.Extensions;

using Panel = StudioFourteen.Panels.Panel;

public partial class SettingsPanel : Panel
{
	private int currentPreset = 0;

	public SettingsPanel()
	{
		this.ResourcePacks.CollectionChanged += this.OnPacksCollectionChanged;
	}

	public FastObservableCollection<ResourcePackReference> ResourcePacks { get; init; } = new();

	public int InspectorPresetIndex
	{
		get => this.currentPreset;
		set
		{
			this.currentPreset = value;
			this.Services.Settings.SetPreset((SettingsService.InterfacePresets)value);
			this.NotifyPropertyChanged();
			this.NotifyPropertyChanged(nameof(SettingsPanel.InspectorPresetTitle));
			this.NotifyPropertyChanged(nameof(SettingsPanel.InspectorPresetBody));
		}
	}

	public string InspectorPresetTitle => StudioFourteen.Resources.Find($"LOC_Settings_Inspectors_Preset_{(SettingsService.InterfacePresets)this.currentPreset}", string.Empty);
	public string InspectorPresetBody => StudioFourteen.Resources.Find($"LOC_Settings_Inspectors_Preset_{(SettingsService.InterfacePresets)this.currentPreset}_Description", string.Empty);

	public static void Show(PanelContextBase context, string? elementName = null)
	{
		ShowAsync(context, elementName).Run();
	}

	public static async Task ShowAsync(PanelContextBase context, string? elementName = null)
	{
		SettingsPanel? panel = await context.SetIsOpenAsync<SettingsPanel>(true, true);
		if (panel == null)
			return;

		if (elementName != null)
		{
			await panel.MainThread();
			panel.ShowElement(elementName);
		}
	}

	public void ShowElement(string elementName)
	{
		object? element = this.FindName(elementName);
		if (element == null)
			return;

		if (element is TabItem tabItem)
		{
			tabItem.IsSelected = true;
		}
	}

	protected override void OnOpened()
	{
		this.ResourcePacks.Replace(this.Services.ResourcePacks.Packs);
		base.OnOpened();
	}

	private async void OnBrosePhotoDirectoryClicked(object sender, RoutedEventArgs e)
	{
		DirectoryInfo? dir = null;
		if (!string.IsNullOrEmpty(this.Settings.PhotoDirectory))
			dir = new DirectoryInfo(this.Settings.PhotoDirectory);

		DirectoryInfo? newDir = await this.Services.Files.ShowDirectoryDialog(dir);

		if (newDir == null)
			return;

		this.Settings.PhotoDirectory = newDir.FullName;
	}

	private void OnPacksCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		this.Services.ResourcePacks.Packs = new(this.ResourcePacks);
	}

	private void OnApplyResourcePacksClicked(object sender, RoutedEventArgs e)
	{
		this.Services.ResourcePacks.Apply();
	}
}
