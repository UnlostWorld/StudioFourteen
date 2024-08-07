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
	[AutoNotify] public string SaveLabel { get; private set; } = "Save Scene";
	[AutoNotify] public FastObservableCollection<CharacterSave> Characters { get; init; } = new();

	[AutoNotify] public string CharactersText => string.Format(ScreenshotStudio.Resources.Find("LOC_Save_CharactersText", string.Empty), 0, this.Characters.Count);

	[AutoNotify] public bool IncludeLocation { get; set; }
	[AutoNotify] public bool IncludeWeather { get; set; }
	[AutoNotify] public bool IncludeTimeOfDay { get; set; }

	[AutoNotify]
	public bool IncludeEnvironment
	{
		get => this.IncludeLocation || this.IncludeWeather || this.IncludeTimeOfDay;
		set
		{
			this.IncludeLocation = value;
			this.IncludeWeather = value;
			this.IncludeTimeOfDay = value;
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

				characters.Add(new(name));
			}
		}

		await this.Dispatcher.MainThread();

		this.Characters.Replace(characters);
	}
}

public class CharacterSave(string name)
	: ViewModel
{
	public string Name { get; init; } = name;

	public string ToolTipText => string.Format(Resources.Find("LOC_Save_IncludeCharacterToolTip", string.Empty), this.Name);
	public string AppearanceToolTipText => string.Format(Resources.Find("LOC_Save_IncludeAppearanceToolTip", string.Empty), this.Name);
	public string PoseToolTipText => string.Format(Resources.Find("LOC_Save_IncludePoseToolTip", string.Empty), this.Name);
	public string ExportPoseToolTipText => string.Format(Resources.Find("LOC_Save_ExportPoseTooltip", string.Empty), this.Name);
	public string ExportAppearanceToolTipText => string.Format(Resources.Find("LOC_Save_ExportAppearanceTooltip", string.Empty), this.Name);

	[AutoNotify] public bool IncludeCharacter { get; set; }
	[AutoNotify] public bool IncludeAppearance { get; set; }
	[AutoNotify] public bool IncludePose { get; set; }
}