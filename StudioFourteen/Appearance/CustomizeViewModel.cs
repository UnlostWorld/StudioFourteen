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

namespace StudioFourteen.Appearance.Customize;

using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.GameData;
using StudioFourteen.GameData.Library;
using StudioFourteen.GameData.Sheets;
using StudioFourteen.Mvm;
using StudioFourteen.Plugin;
using StudioFourteen.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Threading;
using WpfUtils;
using WpfUtils.Extensions;

using CharaMakeType = StudioFourteen.GameData.Sheets.CharaMakeType;

public partial class CustomizeViewModel : ViewModel
{
	private readonly DispatcherObject dispatcher;

	public CustomizeViewModel(DispatcherObject dispatcher)
	{
		this.dispatcher = dispatcher;
		this.Services.Target.TargetChanged += this.OnTargetChanged;
		this.OnTargetChanged();

		if (DalamudServices.Framework == null)
			return;

		DalamudServices.Framework.Update += this.OnFrameworkUpdate;
	}

	public bool HasValidTarget => this.Services.Target.HasValidTarget;

	public FastObservableCollection<MenuViewModel?> BodyMenus { get; init; } = new();
	public FastObservableCollection<MenuViewModel?> HeadMenus { get; init; } = new();
	public FastObservableCollection<MenuViewModel?> MakeupMenus { get; init; } = new();

	private void OnTargetChanged()
	{
		this.UpdateMenus().Run();
	}

	private async Task UpdateMenus()
	{
		await Threads.FrameworkThread();
		List<MenuViewModel?> bodyMenus = new();
		List<MenuViewModel?> headMenus = new();
		List<MenuViewModel?> makeupMenus = new();

		if (DalamudServices.ObjectTable == null)
			return;

		CharaMakeType makeType;
		unsafe
		{
			Character* pCharacter = (Character*)DalamudServices.ObjectTable.GetObjectAddress(this.Services.Target.TargetObjectIndex);
			if (pCharacter == null)
				return;

			CharaMakeType? characterMakeType = pCharacter->GetCharaMakeType();
			if (characterMakeType == null)
				return;

			makeType = characterMakeType.Value;
		}

		RaceRows race = (RaceRows)makeType.Race.RowId;

		bodyMenus.Add(this.GetMenu(makeType, CustomizeIndex.Race));
		bodyMenus.Add(this.GetMenu(makeType, CustomizeIndex.Tribe));
		bodyMenus.Add(this.GetMenu(makeType, CustomizeIndex.Gender));
		bodyMenus.Add(this.GetMenu(makeType, CustomizeIndex.ModelType));
		bodyMenus.Add(this.GetMenu(makeType, CustomizeIndex.Height));

		// Huyr & Roegadyn - Muscle Tone
		if (race == RaceRows.Hyur || race == RaceRows.Roegadyn)
			bodyMenus.Add(this.GetMenu(makeType, CustomizeIndex.RaceFeatureSize));

		bodyMenus.Add(this.GetMenu(makeType, CustomizeIndex.BustSize));

		// Miqote & AuRa - Tail Type and Tail Length
		if (race == RaceRows.Miqote || race == RaceRows.AuRa)
		{
			bodyMenus.Add(this.GetMenu(makeType, CustomizeIndex.RaceFeatureType));
			bodyMenus.Add(this.GetMenu(makeType, CustomizeIndex.RaceFeatureSize));
		}

		bodyMenus.Add(this.GetMenu(makeType, CustomizeIndex.SkinColor));

		// Hrothgar - Fur Pattern
		if (race == RaceRows.Hrothgar)
			bodyMenus.Add(this.GetMenu(makeType, CustomizeIndex.LipColor));

		// Viera & Lalafell & Elezen - Ear shape & length
		if (race == RaceRows.Viera || race == RaceRows.Lalafell || race == RaceRows.Elezen)
		{
			bodyMenus.Add(this.GetMenu(makeType, CustomizeIndex.RaceFeatureType));
			bodyMenus.Add(this.GetMenu(makeType, CustomizeIndex.RaceFeatureSize));
		}

		headMenus.Add(this.GetMenu(makeType, CustomizeIndex.FaceType));
		headMenus.Add(this.GetMenu(makeType, CustomizeIndex.JawShape));
		headMenus.Add(this.GetMenu(makeType, CustomizeIndex.EyeShape));
		headMenus.Add(this.GetMenu(makeType, CustomizeIndex.EyeColor));
		headMenus.Add(this.GetMenu(makeType, CustomizeIndex.EyeColor2));

		if (race == RaceRows.AuRa)
			headMenus.Add(this.GetMenu(makeType, CustomizeIndex.FaceFeaturesColor));

		headMenus.Add(this.GetMenu(makeType, CustomizeIndex.Eyebrows));
		headMenus.Add(this.GetMenu(makeType, CustomizeIndex.NoseShape));
		headMenus.Add(this.GetMenu(makeType, CustomizeIndex.LipStyle)); // lips or fang length

		if (race != RaceRows.Hrothgar)
		{
			makeupMenus.Add(new ToggleMenu(CustomizeIndex.LipStyle, this.GetMenu(makeType, CustomizeIndex.LipColor)));
		}

		makeupMenus.Add(this.GetMenu(makeType, CustomizeIndex.Facepaint));
		////makeupMenus.Add(new ToggleMenu(CustomizeIndex.Facepaint, Resources.Find("LOC_Character_FacePaintToggle", "Flip face paint")));
		makeupMenus.Add(this.GetMenu(makeType, CustomizeIndex.FacepaintColor));
		makeupMenus.Add(this.GetMenu(makeType, CustomizeIndex.FaceFeatures));
		makeupMenus.Add(this.GetMenu(makeType, CustomizeIndex.FaceFeaturesColor));
		makeupMenus.Add(this.GetMenu(makeType, CustomizeIndex.HairStyle));
		makeupMenus.Add(this.GetMenu(makeType, CustomizeIndex.HairColor));
		makeupMenus.Add(new ToggleMenu(CustomizeIndex.HasHighlights, this.GetMenu(makeType, CustomizeIndex.HairColor2)));

		await this.dispatcher.MainThread();

		this.BodyMenus.Replace(bodyMenus);
		this.HeadMenus.Replace(headMenus);
		this.MakeupMenus.Replace(makeupMenus);
	}

	private unsafe void OnFrameworkUpdate(IFramework framework)
	{
		if (!this.Services.Studio.IsOpen)
			return;

		// TODO: Ensure race, tribe, and gender have not changed.
		Character* pTarget = this.Services.Target.GetTarget();
		if (pTarget == null)
			return;

		foreach(MenuViewModel? menu in this.BodyMenus)
		{
			menu?.OnFrameworkUpdate(pTarget);
		}

		foreach (MenuViewModel? menu in this.HeadMenus)
		{
			menu?.OnFrameworkUpdate(pTarget);
		}

		foreach (MenuViewModel? menu in this.MakeupMenus)
		{
			menu?.OnFrameworkUpdate(pTarget);
		}
	}

	private MenuViewModel? GetMenu(CharaMakeType makeType, CustomizeIndex index)
	{
		if (index == CustomizeIndex.Race)
			return new ExcelLibraryEntryMenu<RaceLibraryEntry>(index, Resources.Find("LOC_Character_Race", "Race"));

		if (index == CustomizeIndex.Tribe)
			return new TribeMenu(makeType.Race.Value, Resources.Find("LOC_Character_Tribe", "Tribe"));

		if (index == CustomizeIndex.ModelType)
			return new ModelTypeMenu(makeType.Tribe.Value, Resources.Find("LOC_Character_ModelType", "Model Type"));

		if (index == CustomizeIndex.Gender)
			return new GenderMenu(Resources.Find("LOC_Character_BodyShape", "Body Shape"));

		if (index == CustomizeIndex.HairColor2)
		{
			CharaMakeType.CharaMakeMenu? hairMakeMenu = makeType.GetMenu(CustomizeIndex.HairColor);
			if (hairMakeMenu == null)
				throw new Exception("No hair make menu");

			return new ColorMenu(makeType, hairMakeMenu.Value, index, MenuViewModel.ToggleModes.None, true);
		}

		CharaMakeType.CharaMakeMenu? makeMenu = makeType.GetMenu(index);
		if (makeMenu == null)
			return null;

		if (index == CustomizeIndex.Facepaint)
			return new CustomizeLibraryEntryMenu(makeType, makeMenu.Value, index, true);

		if (index == CustomizeIndex.HairStyle)
			return new CustomizeLibraryEntryMenu(makeType, makeMenu.Value, index, false);

		if (index == CustomizeIndex.FaceFeatures)
			return new MultiIconMenu(makeType, makeMenu.Value, index);

		// Weird usage by SQEX to pack iris size into eye shape, but list it under eye color 2, which
		// is actually controlled by EyeColor's 'double color picker'.
		// thanks guys.
		MenuViewModel.ToggleModes toggleMode = MenuViewModel.ToggleModes.None;
		if (index == CustomizeIndex.EyeColor2)
		{
			toggleMode = MenuViewModel.ToggleModes.IsToggle;
			index = CustomizeIndex.EyeShape;
		}
		else if (index == CustomizeIndex.EyeShape || index == CustomizeIndex.LipStyle)
		{
			toggleMode = MenuViewModel.ToggleModes.IsValue;
		}

		MenuViewModel menu = (MenuTypes)makeMenu.Value.SubMenuType switch
		{
			MenuTypes.ListSelector => new ListMenu(makeMenu.Value, index, toggleMode),
			MenuTypes.IconSelector => new IconMenu(makeMenu.Value, index, toggleMode),
			MenuTypes.ColorPicker => new ColorMenu(makeType, makeMenu.Value, index, toggleMode),
			MenuTypes.DoubleColorPicker => new DoubleColorMenu(makeType, makeMenu.Value, index, toggleMode),
			MenuTypes.Percentage => new PercentageMenu(makeMenu.Value, index, toggleMode),
			_ => throw new NotSupportedException(),
		};

		return menu;
	}
}

#pragma warning disable
public enum MenuTypes
{
	ListSelector = 0,
	IconSelector = 1,
	ColorPicker = 2,
	DoubleColorPicker = 3,
	MultiIconSelector = 4, // Just face features
	Percentage = 5,
}