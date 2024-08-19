namespace ScreenshotStudio.Save;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Windows;
using System.Collections.Generic;
using System.Windows;
using WpfUtils.Extensions;

public partial class SaveWindow : PanelWindow
{
	private readonly Dictionary<int, CharacterViewModel> characterLookup = new();

	[AutoNotify] public bool CanSave { get; private set; } = true;
	[AutoNotify] public string SaveLabel { get; private set; } = string.Empty;
	[AutoNotify] public FastObservableCollection<CharacterViewModel> Characters { get; init; } = new();

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

	public SaveService.SaveConfiguration? Configuration { get; private set; }

	protected override void OnOpened()
	{
		base.OnOpened();
		this.Configuration = this.Services.Save.Current.Copy();
	}

	protected override unsafe void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		if (DalamudServices.ObjectTable == null)
			return;

		for (int i = 0; i < DalamudServices.ObjectTable.Length; ++i)
		{
			bool isValid = this.Services.Save.CanIncludeCharacter(i);

			Character* pCharacter = (Character*)DalamudServices.ObjectTable.GetObjectAddress(i);
			if (pCharacter == null)
			{
				isValid = false;
			}

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
	}
}

public class CharacterViewModel(int objectTableIndex)
	: ViewModel
{
	public int ObjectTableIndex { get; init; } = objectTableIndex;

	[AutoNotify] public string? Nickname => this.Services.Nickname.GetNicknameOrDefault(this.ObjectTableIndex);
	[AutoNotify] public string? Name { get; set; }

	[AutoNotify] public string ToolTipText => string.Format(Resources.Find("LOC_Save_IncludeCharacterToolTip", string.Empty), this.Name, this.Nickname ?? this.Name);
	[AutoNotify] public string ExportPoseToolTipText => string.Format(Resources.Find("LOC_Save_ExportPoseToolTip", string.Empty), this.Name);
	[AutoNotify] public string ExportAppearanceToolTipText => string.Format(Resources.Find("LOC_Save_ExportAppearanceToolTip", string.Empty), this.Name);

	[AutoNotify] public bool IncludeCharacter
	{
		get => this.Services.Save.GetIncludeCharacter(this.ObjectTableIndex);
		set => this.Services.Save.SetIncludeCharacter(this.ObjectTableIndex, value);
	}
}