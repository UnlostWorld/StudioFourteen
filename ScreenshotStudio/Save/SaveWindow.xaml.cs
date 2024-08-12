namespace ScreenshotStudio.Save;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Utilities;
using ScreenshotStudio.Windows;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using WpfUtils;
using WpfUtils.Extensions;

public partial class SaveWindow : PanelWindow
{
	[AutoNotify] public bool CanSave { get; private set; } = true;
	[AutoNotify] public string SaveLabel { get; private set; } = string.Empty;
	[AutoNotify] public FastObservableCollection<CharacterViewModel> Characters { get; init; } = new();

	[AutoNotify] public string CharactersText { get; set; } = string.Empty;

	public SaveService.SaveConfiguration? Configuration { get; private set; }

	protected override void OnOpened()
	{
		base.OnOpened();

		this.Configuration = this.Services.Save.Current.Copy();

		Task.Run(this.Populate);
	}

	private void OnSaveClicked(object sender, RoutedEventArgs e)
	{
		this.Services.Save.Save(this.Configuration);
	}

	private async Task Populate()
	{
		await Threads.FrameworkThread();

		if (DalamudServices.ObjectTable == null)
			return;

		int fromIndex = GroupPoseService.GPoseFirstCharacter;
		int toIndex = GroupPoseService.GPoseFirstCharacter + GroupPoseService.GPoseCharacterCount;

		if (!this.Services.GroupPose.IsGroupPosing)
		{
			fromIndex = 0;
			toIndex = Math.Min(DalamudServices.ObjectTable.Length, GroupPoseService.GPoseFirstCharacter);
		}

		List<CharacterViewModel> characters = new();

		unsafe
		{
			for (int i = fromIndex; i < toIndex; ++i)
			{
				nint? address = DalamudServices.ObjectTable.GetObjectAddress(i);
				if (address == null)
					continue;

				Character* character = (Character*)address;
				if (character == null)
					continue;

				string name = character->GetNameAsString() ?? "Unknown";
				CharacterViewModel vm = new(name);
				vm.IncludeCharacter = i == fromIndex;
				characters.Add(vm);
			}
		}

		await this.Dispatcher.MainThread();

		this.Characters.Replace(characters);

		this.UpdateLabels();
	}

	private void OnChanged(object sender, RoutedEventArgs e)
	{
		this.UpdateLabels();
	}

	private void UpdateLabels()
	{
		int selectedCount = 0;
		foreach(CharacterViewModel vm in this.Characters)
		{
			if (vm.IncludeCharacter)
			{
				selectedCount++;
			}
		}

		this.CharactersText = string.Format(ScreenshotStudio.Resources.Find("LOC_Save_CharactersText", string.Empty), selectedCount, this.Characters.Count);

		this.SaveLabel = ScreenshotStudio.Resources.Find("LOC_Save_SaveScene", "Save");
		this.CanSave = true;

		if (this.Configuration == null)
		{
			this.CanSave = false;
			return;
		}

		if (!this.Configuration.IncludeLocation
			&& !this.Configuration.IncludeWeather
			&& !this.Configuration.IncludeTimeOfDay)
		{
			if (selectedCount == 1)
			{
				if (this.Configuration.IncludePoses && !this.Configuration.IncludeAppearances)
				{
					this.SaveLabel = ScreenshotStudio.Resources.Find("LOC_Save_SavePose", "Save");
				}
				else if (this.Configuration.IncludeAppearances && !this.Configuration.IncludePoses)
				{
					this.SaveLabel = ScreenshotStudio.Resources.Find("LOC_Save_SaveAppearance", "Save");
				}
			}

			if (selectedCount == 0 || (!this.Configuration.IncludePoses && !this.Configuration.IncludeAppearances))
			{
				this.CanSave = false;
			}
		}
	}
}

public class CharacterViewModel(string name)
	: ViewModel
{
	public string Name { get; init; } = name;

	public string ToolTipText => string.Format(Resources.Find("LOC_Save_IncludeCharacterToolTip", string.Empty), this.Name);
	public string ExportPoseToolTipText => string.Format(Resources.Find("LOC_Save_ExportPoseToolTip", string.Empty), this.Name);
	public string ExportAppearanceToolTipText => string.Format(Resources.Find("LOC_Save_ExportAppearanceToolTip", string.Empty), this.Name);

	[AutoNotify] public bool IncludeCharacter { get; set; }
}