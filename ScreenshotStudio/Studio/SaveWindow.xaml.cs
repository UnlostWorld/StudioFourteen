namespace ScreenshotStudio.Studio;

using Dalamud.Utility;
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
	[AutoNotify] public FastObservableCollection<CharacterSave> Characters { get; init; } = new();

	[AutoNotify] public string CharactersText { get; set; } = string.Empty;

	[AutoNotify] public bool IncludeLocation
	{
		get => this.GetPersistence<bool>();
		set
		{
			this.SetPersistence(value);
			this.UpdateLabels();
		}
	}

	[AutoNotify] public bool IncludeWeather
	{
		get => this.GetPersistence<bool>();
		set
		{
			this.SetPersistence(value);
			this.UpdateLabels();
		}
	}

	[AutoNotify] public bool IncludeTimeOfDay
	{
		get => this.GetPersistence<bool>();
		set
		{
			this.SetPersistence(value);
			this.UpdateLabels();
		}
	}

	[AutoNotify] public bool IncludePoses
	{
		get => this.GetPersistence<bool>();
		set
		{
			this.SetPersistence(value);
			this.UpdateLabels();
		}
	}

	[AutoNotify] public bool IncludeAppearances
	{
		get => this.GetPersistence<bool>();
		set
		{
			this.SetPersistence(value);
			this.UpdateLabels();
		}
	}

	protected override void OnOpened()
	{
		base.OnOpened();
		Task.Run(this.Populate);
	}

	private void OnSaveClicked(object sender, RoutedEventArgs e)
	{
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

		List<CharacterSave> characters = new();

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
				CharacterSave vm = new(name);
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
		foreach(CharacterSave vm in this.Characters)
		{
			if (vm.IncludeCharacter)
			{
				selectedCount++;
			}
		}

		this.CharactersText = string.Format(ScreenshotStudio.Resources.Find("LOC_Save_CharactersText", string.Empty), selectedCount, this.Characters.Count);

		this.SaveLabel = ScreenshotStudio.Resources.Find("LOC_Save_SaveScene", "Save");
		this.CanSave = true;

		if (!this.IncludeLocation
			&& !this.IncludeWeather
			&& !this.IncludeTimeOfDay)
		{
			if (selectedCount == 1)
			{
				if (this.IncludePoses && !this.IncludeAppearances)
				{
					this.SaveLabel = ScreenshotStudio.Resources.Find("LOC_Save_SavePose", "Save");
				}
				else if (this.IncludeAppearances && !this.IncludePoses)
				{
					this.SaveLabel = ScreenshotStudio.Resources.Find("LOC_Save_SaveAppearance", "Save");
				}
			}

			if (selectedCount == 0 || (!this.IncludePoses && !this.IncludeAppearances))
			{
				this.CanSave = false;
			}
		}
	}
}

public class CharacterSave(string name)
	: ViewModel
{
	public string Name { get; init; } = name;

	public string ToolTipText => string.Format(Resources.Find("LOC_Save_IncludeCharacterToolTip", string.Empty), this.Name);
	public string ExportPoseToolTipText => string.Format(Resources.Find("LOC_Save_ExportPoseToolTip", string.Empty), this.Name);
	public string ExportAppearanceToolTipText => string.Format(Resources.Find("LOC_Save_ExportAppearanceToolTip", string.Empty), this.Name);

	[AutoNotify] public bool IncludeCharacter { get; set; }
}