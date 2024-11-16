namespace StudioFourteen.Appearance.Customize;

using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.GameData;
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

		headMenus.Add(this.GetMenu(makeType, CustomizeIndex.FaceType));
		headMenus.Add(this.GetMenu(makeType, CustomizeIndex.JawShape));
		headMenus.Add(this.GetMenu(makeType, CustomizeIndex.EyeShape));
		headMenus.Add(this.GetMenu(makeType, CustomizeIndex.EyeColor));
		headMenus.Add(this.GetMenu(makeType, CustomizeIndex.EyeColor2));

		if (race == RaceRows.AuRa)
			headMenus.Add(this.GetMenu(makeType, CustomizeIndex.FaceFeaturesColor));

		headMenus.Add(this.GetMenu(makeType, CustomizeIndex.JawShape));
		headMenus.Add(this.GetMenu(makeType, CustomizeIndex.Eyebrows));
		headMenus.Add(this.GetMenu(makeType, CustomizeIndex.NoseShape));
		headMenus.Add(this.GetMenu(makeType, CustomizeIndex.LipStyle)); // lips or fang length

		if (race == RaceRows.Viera || race == RaceRows.Lalafell || race == RaceRows.Elezen)
		{
			headMenus.Add(this.GetMenu(makeType, CustomizeIndex.RaceFeatureType));
			headMenus.Add(this.GetMenu(makeType, CustomizeIndex.RaceFeatureSize));
		}

		if (race != RaceRows.Hrothgar)
			makeupMenus.Add(this.GetMenu(makeType, CustomizeIndex.LipColor));

		makeupMenus.Add(this.GetMenu(makeType, CustomizeIndex.Facepaint));
		makeupMenus.Add(this.GetMenu(makeType, CustomizeIndex.FacepaintColor));
		makeupMenus.Add(this.GetMenu(makeType, CustomizeIndex.FaceFeatures));
		makeupMenus.Add(this.GetMenu(makeType, CustomizeIndex.FaceFeaturesColor));
		makeupMenus.Add(this.GetMenu(makeType, CustomizeIndex.HairStyle));
		makeupMenus.Add(this.GetMenu(makeType, CustomizeIndex.HairColor));
		makeupMenus.Add(this.GetMenu(makeType, CustomizeIndex.HairColor2));

		await this.dispatcher.MainThread();

		this.BodyMenus.Replace(bodyMenus);
		this.HeadMenus.Replace(headMenus);
		this.MakeupMenus.Replace(makeupMenus);
	}

	private unsafe void OnFrameworkUpdate(IFramework framework)
	{
		// TODO: Ensure race, tribe, and gender have not changed.
		Character* pTarget = this.Services.Target.GetTarget();
		if (pTarget == null)
			return;

		foreach(MenuViewModel? menu in this.BodyMenus)
		{
			if (menu == null)
				continue;

			menu.OnFrameworkUpdate(pTarget);
		}
	}

	private MenuViewModel? GetMenu(CharaMakeType makeType, CustomizeIndex index)
	{
		if (index == CustomizeIndex.Race)
			return new RaceMenu();

		if (index == CustomizeIndex.Tribe)
			return new TribeMenu(makeType.Race.Value);

		if (index == CustomizeIndex.ModelType)
			return new ModelTypeMenu(makeType.Tribe.Value);

		CharaMakeType.CharaMakeMenu? makeMenu = makeType.GetMenu(index);
		if (makeMenu == null)
			return null;

		MenuViewModel menu = (MenuTypes)makeMenu.Value.SubMenuType switch
		{
			MenuTypes.ListSelector => new ListMenu(makeMenu.Value, index),
			MenuTypes.IconSelector => new IconMenu(makeMenu.Value, index),
			MenuTypes.ColorPicker => new ColorMenu(makeType, makeMenu.Value, index),
			MenuTypes.DoubleColorPicker => new DoubleColorMenu(makeMenu.Value, index),
			MenuTypes.MultiIconSelector => new MultiColorMenu(makeMenu.Value, index),
			MenuTypes.Percentage => new PercentageMenu(makeMenu.Value, index),
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
	MultiIconSelector = 4,
	Percentage = 5,
}