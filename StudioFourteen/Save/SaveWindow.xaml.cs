namespace ScreenshotStudio.Save;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Data;
using ScreenshotStudio.Library.Sources;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Tags;
using ScreenshotStudio.Panels;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using WpfUtils.Extensions;
using ScreenshotStudio.Mvm;

public partial class SaveWindow : Panel
{
	private readonly Dictionary<int, CharacterViewModel> characterLookup = new();
	private RecentDirectoryViewModel? selectedDirectory;

	[AutoNotify] public bool CanSave { get; private set; } = true;
	[AutoNotify] public string SaveLabel { get; private set; } = string.Empty;
	[AutoNotify] public FastObservableCollection<CharacterViewModel> Characters { get; init; } = new();
	[AutoNotify] public FastObservableCollection<RecentEntryViewModel> RecentDirectories { get; init; } = new();

	[AutoNotify] public string FileName
	{
		get => Path.GetFileNameWithoutExtension(this.Services.Save.SaveFileInfo?.Name) ?? string.Empty;
		set => this.Services.Save.SetSaveFileInfo(null, value);
	}

	[AutoNotify] public RecentEntryViewModel? SelectedDirectory
	{
		get => this.selectedDirectory;
		set
		{
			if (value is AddDirectoryViewModel addViewModel)
			{
				this.OnBrowseDirectoryClicked(addViewModel);
			}
			else if (value is RecentDirectoryViewModel viewModel)
			{
				this.selectedDirectory = viewModel;

				this.Services.Save.SetSaveFileInfo(this.selectedDirectory.Directory, null);
			}
		}
	}

	[AutoNotify] public string CharactersText
	{
		get
		{
			int selectedCount = 0;
			foreach (CharacterViewModel vm in this.Characters)
			{
				if (vm.IncludeCharacter)
				{
					selectedCount++;
				}
			}

			return string.Format(ScreenshotStudio.Resources.Find("LOC_Save_CharactersText", string.Empty), selectedCount, this.Characters.Count);
		}
	}

	public SaveService.SaveConfiguration? Configuration => this.Services.Save.Current;

	protected override void OnOpened()
	{
		base.OnOpened();

		this.RecentDirectories.Clear();
		foreach(SourceBase source in this.Services.Library.Sources)
		{
			if (source is FileSource fileSource)
			{
				if (fileSource.Directory == null)
					continue;

				RecentDirectoryViewModel viewModel = new RecentDirectoryViewModel(fileSource.Directory);
				this.RecentDirectories.Add(viewModel);

				if (fileSource.Directory.IsDirectory(this.Services.Save.SaveFileInfo?.Directory))
				{
					this.selectedDirectory = viewModel;
				}
			}
		}

		if (this.Services.Save.SaveFileInfo?.Directory != null && this.selectedDirectory == null)
		{
			RecentDirectoryViewModel viewModel = new RecentDirectoryViewModel(this.Services.Save.SaveFileInfo.Directory);
			this.RecentDirectories.Add(viewModel);
			this.selectedDirectory = viewModel;
		}

		this.RecentDirectories.Add(new AddDirectoryViewModel());
	}

	protected override unsafe void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		if (DalamudServices.ObjectTable == null)
			return;

		for (int i = 0; i < DalamudServices.ObjectTable.Length; ++i)
		{
			bool isValid = true;

			Character* pCharacter = (Character*)DalamudServices.ObjectTable.GetObjectAddress(i);
			isValid = this.Services.Save.CanInclude(pCharacter);

			if (!isValid && this.characterLookup.ContainsKey(i))
			{
				CharacterViewModel vm = this.characterLookup[i];
				this.Dispatcher.Invoke(() => this.Characters.Remove(vm));

				this.characterLookup.Remove(i);
				continue;
			}
			else if (isValid && !this.characterLookup.ContainsKey(i))
			{
				CharacterViewModel vm = new(i);
				this.characterLookup.Add(i, vm);
				this.Dispatcher.Invoke(() => this.Characters.Add(vm));
			}
			else if (isValid && this.characterLookup.ContainsKey(i))
			{
				this.characterLookup[i].Name = pCharacter->GetDisplayName();
			}
		}
	}

	private void OnSaveClicked(object sender, RoutedEventArgs e)
	{
		this.Services.Save.Save(this.Configuration);
		this.Close();
	}

	private async void OnBrowseDirectoryClicked(AddDirectoryViewModel viewModel)
	{
		if (viewModel.IsAdding)
			return;

		viewModel.IsAdding = true;

		RecentDirectoryViewModel? currentViewModel = this.selectedDirectory;
		this.selectedDirectory = null;

		DirectoryInfo? dir = await this.Services.Files.ShowDirectoryDialog(currentViewModel?.Directory);
		if (dir != null)
		{
			bool exists = false;
			foreach(RecentEntryViewModel vm in this.RecentDirectories)
			{
				if (vm is RecentDirectoryViewModel directoryViewModel && directoryViewModel.Directory.IsDirectory(dir))
				{
					exists = true;
					break;
				}
			}

			if (!exists)
			{
				RecentDirectoryViewModel newDirectoryViewModel = new(dir);
				this.RecentDirectories.Insert(this.RecentDirectories.Count - 1, newDirectoryViewModel);
				currentViewModel = newDirectoryViewModel;
			}
		}

		viewModel.IsAdding = false;
		this.selectedDirectory = currentViewModel;
	}
}

public class RecentEntryViewModel
{
}

public class RecentDirectoryViewModel(DirectoryInfo directory)
	: RecentEntryViewModel
{
	public DirectoryInfo Directory => directory;
}

public class AddDirectoryViewModel
	: RecentEntryViewModel
{
	public bool IsAdding { get; set; }
}

public class CharacterViewModel(int objectTableIndex)
	: ViewModel
{
	public int ObjectTableIndex { get; init; } = objectTableIndex;

	[AutoNotify] public string? Name { get; set; }

	[AutoNotify] public string ToolTipText => string.Format(Resources.Find("LOC_Save_IncludeCharacterToolTip", string.Empty), this.Name, this.Role ?? this.Name);
	[AutoNotify] public string ExportPoseToolTipText => string.Format(Resources.Find("LOC_Save_ExportPoseToolTip", string.Empty), this.Name);
	[AutoNotify] public string ExportAppearanceToolTipText => string.Format(Resources.Find("LOC_Save_ExportAppearanceToolTip", string.Empty), this.Name);

	[AutoNotify] public bool IncludeCharacter
	{
		get => this.Services.Save.GetIncludeCharacter(this.ObjectTableIndex);
		set => this.Services.Save.SetIncludeCharacter(this.ObjectTableIndex, value);
	}

	[AutoNotify] public string? Role
	{
		get => this.Services.Roles.GetRoleOrDefault(this.ObjectTableIndex);
		set => this.Services.Roles.SetRole(this.ObjectTableIndex, value);
	}
}