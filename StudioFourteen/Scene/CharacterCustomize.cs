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

namespace StudioFourteen.Scene;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using Lumina.Excel.Sheets;
using StudioFourteen.Scene.CharacterCustomizeMenus;
using StudioFourteen.Services.Library.GameData;
using StudioFourteen.Services.Library.GameData.Library;
using StudioFourteen.Services.Library.GameData.Sheets;
using StudioFourteen.Services.Tick;

using CharaMakeType = StudioFourteen.Services.Library.GameData.Sheets.CharaMakeType;
using XivCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

public partial class CharacterCustomize(int objectIndex)
	: CharacterBase(objectIndex)
{
	private byte race = 255;
	private byte tribe = 255;

	public enum MenuTypes
	{
		ListSelector = 0,
		IconSelector = 1,
		ColorPicker = 2,
		DoubleColorPicker = 3,
		MultiIconSelector = 4, // Just face features ?
		Percentage = 5,
	}

	// Body
	[ObservableProperty] public partial CharacterCustomizeMenu? Race { get; set; }
	[ObservableProperty] public partial CharacterCustomizeMenu? Tribe { get; set; }
	[ObservableProperty] public partial CharacterCustomizeMenu? Gender { get; set; }
	[ObservableProperty] public partial CharacterCustomizeMenu? ModelType { get; set; }
	[ObservableProperty] public partial CharacterCustomizeMenu? Height { get; set; }
	[ObservableProperty] public partial CharacterCustomizeMenu? MuscleTone { get; set; }
	[ObservableProperty] public partial CharacterCustomizeMenu? BustSize { get; set; }
	[ObservableProperty] public partial CharacterCustomizeMenu? TailType { get; set; }
	[ObservableProperty] public partial CharacterCustomizeMenu? TailLength { get; set; }
	[ObservableProperty] public partial CharacterCustomizeMenu? SkinColor { get; set; }
	[ObservableProperty] public partial CharacterCustomizeMenu? FurPattern { get; set; }
	[ObservableProperty] public partial CharacterCustomizeMenu? EarShape { get; set; }
	[ObservableProperty] public partial CharacterCustomizeMenu? EarLength { get; set; }

	// Face
	[ObservableProperty] public partial CharacterCustomizeMenu? FaceType { get; set; }
	[ObservableProperty] public partial CharacterCustomizeMenu? JawShape { get; set; }
	[ObservableProperty] public partial CharacterCustomizeMenu? EyeShape { get; set; }
	[ObservableProperty] public partial CharacterCustomizeMenu? EyeColor { get; set; }
	[ObservableProperty] public partial CharacterCustomizeMenu? EyeColor2 { get; set; }
	[ObservableProperty] public partial CharacterCustomizeMenu? LimbalRing { get; set; }
	[ObservableProperty] public partial CharacterCustomizeMenu? Eyebrows { get; set; }
	[ObservableProperty] public partial CharacterCustomizeMenu? NoseShape { get; set; }
	[ObservableProperty] public partial CharacterCustomizeMenu? LipStyle { get; set; }

	// Makeup
	[ObservableProperty] public partial CharacterCustomizeMenu? LipColor { get; set; }
	[ObservableProperty] public partial CharacterCustomizeMenu? FacePaint { get; set; }
	[ObservableProperty] public partial CharacterCustomizeMenu? FacepaintColor { get; set; }
	[ObservableProperty] public partial CharacterCustomizeMenu? FaceFeatures { get; set; }
	[ObservableProperty] public partial CharacterCustomizeMenu? FaceFeaturesColor { get; set; }
	[ObservableProperty] public partial CharacterCustomizeMenu? HairStyle { get; set; }
	[ObservableProperty] public partial CharacterCustomizeMenu? HairColor { get; set; }
	[ObservableProperty] public partial CharacterCustomizeMenu? HairColor2 { get; set; }

	public Race? GetRace() => Studio.DataManager.GetRow<Race>(this.GetCustomizeValue(CustomizeIndex.Race));
	public Tribe? GetTribe() => Studio.DataManager.GetRow<Tribe>(this.GetCustomizeValue(CustomizeIndex.Tribe));

	public unsafe byte GetCustomizeValue(CustomizeIndex index)
	{
		TickService.VerifyGameTickThread();
		XivCharacter* pCharacter = this.GetXivCharacter();
		if (pCharacter == null)
			return 0;

		return pCharacter->DrawData.CustomizeData.GetValue(index);
	}

	public unsafe void SetCustomizeValue(CustomizeIndex index, byte value, UpdateSource source)
	{
		TickService.VerifyGameTickThread();

		XivCharacter* pCharacter = this.GetXivCharacter();

		byte oldValue = pCharacter->DrawData.CustomizeData.GetValue(index);
		if (oldValue == value)
			return;

		if (source != UpdateSource.Restore && source != UpdateSource.Preview)
			this.BackupAppearance();

		pCharacter->DrawData.CustomizeData.SetValue(index, value);

		if (index == CustomizeIndex.Race
			|| index == CustomizeIndex.Tribe
			|| index == CustomizeIndex.ModelType
			|| index == CustomizeIndex.Gender)
		{
			Studio.Redraw.Redraw(this);
		}

		this.UpdateCustomize(null, source);
	}

	public unsafe void SetCustomize(CustomizeData customize, UpdateSource source)
	{
		XivCharacter* pCharacter = this.GetXivCharacter();

		if (pCharacter->DrawData.CustomizeData[(int)CustomizeIndex.Race] != customize[(int)CustomizeIndex.Race]
			|| pCharacter->DrawData.CustomizeData[(int)CustomizeIndex.Tribe] != customize[(int)CustomizeIndex.Tribe]
			|| pCharacter->DrawData.CustomizeData[(int)CustomizeIndex.ModelType] != customize[(int)CustomizeIndex.ModelType])
		{
			Studio.Redraw.Redraw(this);
		}

		this.UpdateCustomize(customize, source);
	}

	public unsafe override void OnGameTick()
	{
		base.OnGameTick();

		XivCharacter* pCharacter = this.GetXivCharacter();
		if (pCharacter == null)
			return;

		byte race = this.GetCustomizeValue(CustomizeIndex.Race);
		byte tribe = this.GetCustomizeValue(CustomizeIndex.Race);

		if (race != this.race || tribe != this.tribe)
		{
			this.race = race;
			this.tribe = tribe;
			this.OnRaceOrTribeChanged();
		}

		this.Race?.OnGameTick(this);
		this.Tribe?.OnGameTick(this);
		this.Gender?.OnGameTick(this);
		this.ModelType?.OnGameTick(this);
		this.Height?.OnGameTick(this);
		this.MuscleTone?.OnGameTick(this);
		this.BustSize?.OnGameTick(this);
		this.TailType?.OnGameTick(this);
		this.TailLength?.OnGameTick(this);
		this.SkinColor?.OnGameTick(this);
		this.FurPattern?.OnGameTick(this);
		this.EarShape?.OnGameTick(this);
		this.EarLength?.OnGameTick(this);
		this.FaceType?.OnGameTick(this);
		this.JawShape?.OnGameTick(this);
		this.EyeShape?.OnGameTick(this);
		this.EyeColor?.OnGameTick(this);
		this.EyeColor2?.OnGameTick(this);
		this.LimbalRing?.OnGameTick(this);
		this.Eyebrows?.OnGameTick(this);
		this.NoseShape?.OnGameTick(this);
		this.LipStyle?.OnGameTick(this);
		this.LipColor?.OnGameTick(this);
		this.FacePaint?.OnGameTick(this);
		this.FacepaintColor?.OnGameTick(this);
		this.FaceFeatures?.OnGameTick(this);
		this.FaceFeaturesColor?.OnGameTick(this);
		this.HairStyle?.OnGameTick(this);
		this.HairColor?.OnGameTick(this);
		this.HairColor2?.OnGameTick(this);
	}

	protected unsafe virtual void OnRaceOrTribeChanged()
	{
		TickService.VerifyGameTickThread();

		Studio.Log.Information("update menus!");

		XivCharacter* pCharacter = this.GetXivCharacter();
		CharaMakeType? makeType = pCharacter->DrawData.CustomizeData.GetMakeType();
		this.UpdateMenus(makeType);
	}

	private unsafe void UpdateCustomize(CustomizeData? customize, UpdateSource source)
	{
		TickService.VerifyGameTickThread();

		XivCharacter* pCharacter = this.GetXivCharacter();

		if (source != UpdateSource.Restore)
			this.BackupAppearance();

		CustomizeData* custom = &pCharacter->DrawData.CustomizeData;

		if (customize != null)
			custom->Import(customize.Value);

		bool didLoad = ((Human*)pCharacter->DrawObject)->UpdateDrawData((byte*)custom, true);
		if (!didLoad)
		{
			Studio.Redraw.Redraw(this);
		}
	}

	private void UpdateMenus(CharaMakeType? makeType)
	{
		this.Race = null;
		this.Tribe = null;
		this.Gender = null;
		this.ModelType = null;
		this.Height = null;
		this.MuscleTone = null;
		this.BustSize = null;
		this.TailType = null;
		this.TailLength = null;
		this.SkinColor = null;
		this.FurPattern = null;
		this.EarShape = null;
		this.EarLength = null;
		this.FaceType = null;
		this.JawShape = null;
		this.EyeShape = null;
		this.EyeColor = null;
		this.EyeColor2 = null;
		this.LimbalRing = null;
		this.Eyebrows = null;
		this.NoseShape = null;
		this.LipStyle = null;
		this.LipColor = null;
		this.FacePaint = null;
		this.FacepaintColor = null;
		this.FaceFeatures = null;
		this.FaceFeaturesColor = null;
		this.HairStyle = null;
		this.HairColor = null;
		this.HairColor2 = null;

		if (makeType == null)
			return;

		try
		{
			this.UpdateMenus(makeType.Value);
		}
		catch (Exception ex)
		{
			Studio.Log.Error(ex, "Error updating customize menus");
		}
	}

	private void UpdateMenus(CharaMakeType makeType)
	{
		RaceRows race = (RaceRows)makeType.Race.RowId;

		// Body
		this.Race = this.GetMenu(makeType, CustomizeIndex.Race);
		this.Tribe = this.GetMenu(makeType, CustomizeIndex.Tribe);
		this.Gender = this.GetMenu(makeType, CustomizeIndex.Gender);
		this.ModelType = this.GetMenu(makeType, CustomizeIndex.ModelType);
		this.Height = this.GetMenu(makeType, CustomizeIndex.Height);

		// Huyr & Roegadyn - Muscle Tone
		if (race == RaceRows.Hyur || race == RaceRows.Roegadyn)
			this.MuscleTone = this.GetMenu(makeType, CustomizeIndex.RaceFeatureSize);

		this.BustSize = this.GetMenu(makeType, CustomizeIndex.BustSize);

		// Miqote & AuRa - Tail Type and Tail Length
		if (race == RaceRows.Miqote || race == RaceRows.AuRa)
		{
			this.TailType = this.GetMenu(makeType, CustomizeIndex.RaceFeatureType);
			this.TailLength = this.GetMenu(makeType, CustomizeIndex.RaceFeatureSize);
		}

		this.SkinColor = this.GetMenu(makeType, CustomizeIndex.SkinColor);

		// Hrothgar - Fur Pattern
		if (race == RaceRows.Hrothgar)
			this.FurPattern = this.GetMenu(makeType, CustomizeIndex.LipColor);

		// Viera & Lalafell & Elezen - Ear shape & length
		if (race == RaceRows.Viera || race == RaceRows.Lalafell || race == RaceRows.Elezen)
		{
			this.EarShape = this.GetMenu(makeType, CustomizeIndex.RaceFeatureType);
			this.EarLength = this.GetMenu(makeType, CustomizeIndex.RaceFeatureSize);
		}

		// Face
		this.FaceType = this.GetMenu(makeType, CustomizeIndex.FaceType);
		this.JawShape = this.GetMenu(makeType, CustomizeIndex.JawShape);
		this.EyeShape = this.GetMenu(makeType, CustomizeIndex.EyeShape);
		this.EyeColor = this.GetMenu(makeType, CustomizeIndex.EyeColor);
		this.EyeColor2 = this.GetMenu(makeType, CustomizeIndex.EyeColor2);

		// AuRa - Limbal Ring
		if (race == RaceRows.AuRa)
			this.LimbalRing = this.GetMenu(makeType, CustomizeIndex.FaceFeaturesColor);

		this.Eyebrows = this.GetMenu(makeType, CustomizeIndex.Eyebrows);
		this.NoseShape = this.GetMenu(makeType, CustomizeIndex.NoseShape);
		this.LipStyle = this.GetMenu(makeType, CustomizeIndex.LipStyle); // lips or fang length

		if (race != RaceRows.Hrothgar)
		{
			this.LipColor = new ToggleMenu(CustomizeIndex.LipStyle, this.GetMenu(makeType, CustomizeIndex.LipColor));
		}

		this.FacePaint = this.GetMenu(makeType, CustomizeIndex.Facepaint);
		////makeupMenus.Add(new ToggleMenu(CustomizeIndex.Facepaint, "Flip face paint"));
		this.FacepaintColor = this.GetMenu(makeType, CustomizeIndex.FacepaintColor);
		this.FaceFeatures = this.GetMenu(makeType, CustomizeIndex.FaceFeatures);
		this.FaceFeaturesColor = this.GetMenu(makeType, CustomizeIndex.FaceFeaturesColor);
		this.HairStyle = this.GetMenu(makeType, CustomizeIndex.HairStyle);
		this.HairColor = this.GetMenu(makeType, CustomizeIndex.HairColor);
		this.HairColor2 = new ToggleMenu(CustomizeIndex.HasHighlights, this.GetMenu(makeType, CustomizeIndex.HairColor2));
	}

	private CharacterCustomizeMenu? GetMenu(CharaMakeType makeType, CustomizeIndex index)
	{
		if (index == CustomizeIndex.Race)
			return new ExcelLibraryEntryMenu<RaceLibraryEntry>(index, "Race");

		if (index == CustomizeIndex.Tribe)
			return new TribeMenu(makeType.Race.Value, "Tribe");

		if (index == CustomizeIndex.ModelType)
			return new ModelTypeMenu(makeType.Tribe.Value, "Model Type");

		if (index == CustomizeIndex.Gender)
			return new GenderMenu("Body Shape");

		if (index == CustomizeIndex.HairColor2)
		{
			CharaMakeType.CharaMakeMenu? hairMakeMenu = makeType.GetMenu(CustomizeIndex.HairColor);
			if (hairMakeMenu == null)
				throw new Exception("No hair make menu");

			return new ColorMenu(makeType, hairMakeMenu.Value, index, CharacterCustomizeMenu.ToggleModes.None, true);
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
		CharacterCustomizeMenu.ToggleModes toggleMode = CharacterCustomizeMenu.ToggleModes.None;
		if (index == CustomizeIndex.EyeColor2)
		{
			toggleMode = CharacterCustomizeMenu.ToggleModes.IsToggle;
			index = CustomizeIndex.EyeShape;
		}
		else if (index == CustomizeIndex.EyeShape || index == CustomizeIndex.LipStyle)
		{
			toggleMode = CharacterCustomizeMenu.ToggleModes.IsValue;
		}

		CharacterCustomizeMenu menu = (MenuTypes)makeMenu.Value.SubMenuType switch
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
