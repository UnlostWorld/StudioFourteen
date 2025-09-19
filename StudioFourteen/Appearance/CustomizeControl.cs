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
using DependencyPropertyGenerator;

using Serilog;
using StudioFourteen.GameData;
using StudioFourteen.GameData.Library;
using StudioFourteen.GameData.Sheets;
using StudioFourteen.Scene.GameObjects.Characters;
using StudioFourteen.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfUtils;
using WpfUtils.Extensions;

using CharaMakeType = StudioFourteen.GameData.Sheets.CharaMakeType;
using XivCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

[DependencyProperty<Character>("Character")]
[DependencyProperty<Categories>("Category")]
public partial class CustomizeControl : ItemsControl
{
	protected readonly ILogger Log;
	private readonly List<MenuViewModel?> menus = new();

	private bool isUpdatingMenus = false;
	private Character? character = null;

	public CustomizeControl()
	{
		this.Log = Logging.ForContext<CustomizeControl>();

		this.Loaded += this.OnLoaded;
		this.Unloaded += this.OnUnloaded;
		this.Dispatcher.ShutdownStarted += this.OnShutdown;
	}

	public enum Categories
	{
		Body,
		Face,
		Makeup,
	}

	protected ServiceManager Services => ServiceManager.Instance;

	public override string ToString()
	{
		return "CustomizeControl";
	}

	partial void OnCharacterChanged(Character? newValue)
	{
		this.UpdateMenus();
		this.character = newValue;
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);
	}

	private void OnUnloaded(object? sender, RoutedEventArgs? e)
	{
		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);
	}

	private void OnShutdown(object? sender, EventArgs e)
	{
		this.OnUnloaded(null, null);
	}

	private void OnGameTick()
	{
		if (this.isUpdatingMenus || this.character == null)
			return;

		if (!this.character.CanDraw())
			return;

		// TODO: Ensure race, tribe, and gender have not changed.
		foreach (MenuViewModel? menu in this.menus)
		{
			menu?.OnGameTick(this.character);
		}
	}

	private void UpdateMenus()
	{
		this.UpdateMenusAsync().RunAsynchronously();
	}

	private async Task UpdateMenusAsync()
	{
		try
		{
			this.isUpdatingMenus = true;

			await this.MainThread();
			Character? character = this.Character;
			if (character == null)
				return;

			this.menus.Clear();
			this.Items.Clear();

			bool canDraw = false;
			while (!canDraw)
			{
				await Task.Delay(100);
				await TickService.GameTick();

				canDraw = character.CanDraw();
			}

			await TickService.GameTick();

			CharaMakeType? makeType = character.GetCharaMakeType();
			if (makeType == null)
				throw new Exception("Failed to find character Make Type for target");

			await this.UpdateMenus((CharaMakeType)makeType);
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error updating customize menus");
		}
	}

	private async Task UpdateMenus(CharaMakeType makeType)
	{
		RaceRows race = (RaceRows)makeType.Race.RowId;

		await this.MainThread();
		Categories category = this.Category;

		if (this.Category == Categories.Body)
		{
			this.menus.Add(this.GetMenu(makeType, CustomizeIndex.Race));
			this.menus.Add(this.GetMenu(makeType, CustomizeIndex.Tribe));
			this.menus.Add(this.GetMenu(makeType, CustomizeIndex.Gender));
			this.menus.Add(this.GetMenu(makeType, CustomizeIndex.ModelType));
			this.menus.Add(this.GetMenu(makeType, CustomizeIndex.Height));

			// Huyr & Roegadyn - Muscle Tone
			if (race == RaceRows.Hyur || race == RaceRows.Roegadyn)
				this.menus.Add(this.GetMenu(makeType, CustomizeIndex.RaceFeatureSize));

			this.menus.Add(this.GetMenu(makeType, CustomizeIndex.BustSize));

			// Miqote & AuRa - Tail Type and Tail Length
			if (race == RaceRows.Miqote || race == RaceRows.AuRa)
			{
				this.menus.Add(this.GetMenu(makeType, CustomizeIndex.RaceFeatureType));
				this.menus.Add(this.GetMenu(makeType, CustomizeIndex.RaceFeatureSize));
			}

			this.menus.Add(this.GetMenu(makeType, CustomizeIndex.SkinColor));

			// Hrothgar - Fur Pattern
			if (race == RaceRows.Hrothgar)
				this.menus.Add(this.GetMenu(makeType, CustomizeIndex.LipColor));

			// Viera & Lalafell & Elezen - Ear shape & length
			if (race == RaceRows.Viera || race == RaceRows.Lalafell || race == RaceRows.Elezen)
			{
				this.menus.Add(this.GetMenu(makeType, CustomizeIndex.RaceFeatureType));
				this.menus.Add(this.GetMenu(makeType, CustomizeIndex.RaceFeatureSize));
			}
		}
		else if (this.Category == Categories.Face)
		{
			this.menus.Add(this.GetMenu(makeType, CustomizeIndex.FaceType));
			this.menus.Add(this.GetMenu(makeType, CustomizeIndex.JawShape));
			this.menus.Add(this.GetMenu(makeType, CustomizeIndex.EyeShape));
			this.menus.Add(this.GetMenu(makeType, CustomizeIndex.EyeColor));
			this.menus.Add(this.GetMenu(makeType, CustomizeIndex.EyeColor2));

			if (race == RaceRows.AuRa)
				this.menus.Add(this.GetMenu(makeType, CustomizeIndex.FaceFeaturesColor));

			this.menus.Add(this.GetMenu(makeType, CustomizeIndex.Eyebrows));
			this.menus.Add(this.GetMenu(makeType, CustomizeIndex.NoseShape));
			this.menus.Add(this.GetMenu(makeType, CustomizeIndex.LipStyle)); // lips or fang length
		}
		else if (this.Category == Categories.Makeup)
		{
			if (race != RaceRows.Hrothgar)
			{
				this.menus.Add(new ToggleMenu(CustomizeIndex.LipStyle, this.GetMenu(makeType, CustomizeIndex.LipColor)));
			}

			this.menus.Add(this.GetMenu(makeType, CustomizeIndex.Facepaint));
			////makeupMenus.Add(new ToggleMenu(CustomizeIndex.Facepaint, Resources.Find("LOC_Character_FacePaintToggle", "Flip face paint")));
			this.menus.Add(this.GetMenu(makeType, CustomizeIndex.FacepaintColor));
			this.menus.Add(this.GetMenu(makeType, CustomizeIndex.FaceFeatures));
			this.menus.Add(this.GetMenu(makeType, CustomizeIndex.FaceFeaturesColor));
			this.menus.Add(this.GetMenu(makeType, CustomizeIndex.HairStyle));
			this.menus.Add(this.GetMenu(makeType, CustomizeIndex.HairColor));
			this.menus.Add(new ToggleMenu(CustomizeIndex.HasHighlights, this.GetMenu(makeType, CustomizeIndex.HairColor2)));
		}

		foreach (MenuViewModel? menu in this.menus)
		{
			this.Items.Add(menu);
		}

		this.isUpdatingMenus = false;
	}

	private MenuViewModel? GetMenu(CharaMakeType makeType, CustomizeIndex index)
	{
		if (index == CustomizeIndex.Race)
			return new ExcelLibraryEntryMenu<RaceLibraryEntry>(index, StudioFourteen.Resources.Find("LOC_Character_Race", "Race"));

		if (index == CustomizeIndex.Tribe)
			return new TribeMenu(makeType.Race.Value, StudioFourteen.Resources.Find("LOC_Character_Tribe", "Tribe"));

		if (index == CustomizeIndex.ModelType)
			return new ModelTypeMenu(makeType.Tribe.Value, StudioFourteen.Resources.Find("LOC_Character_ModelType", "Model Type"));

		if (index == CustomizeIndex.Gender)
			return new GenderMenu(StudioFourteen.Resources.Find("LOC_Character_BodyShape", "Body Shape"));

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