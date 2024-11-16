namespace StudioFourteen.Appearance.Customize;

using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
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

	private void OnTargetChanged()
	{
		this.UpdateMenus().Run();

		this.BodyMenus.Clear();
	}

	private async Task UpdateMenus()
	{
		await Threads.FrameworkThread();
		List<MenuViewModel?> menus = new();

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

		menus.Add(this.GetMenu(makeType, CustomizeIndex.Race));
		menus.Add(this.GetMenu(makeType, CustomizeIndex.Tribe));
		menus.Add(this.GetMenu(makeType, CustomizeIndex.Gender));
		menus.Add(this.GetMenu(makeType, CustomizeIndex.ModelType));
		menus.Add(this.GetMenu(makeType, CustomizeIndex.Height));
		menus.Add(this.GetMenu(makeType, CustomizeIndex.RaceFeatureSize));
		menus.Add(this.GetMenu(makeType, CustomizeIndex.BustSize));
		menus.Add(this.GetMenu(makeType, CustomizeIndex.RaceFeatureType));
		menus.Add(this.GetMenu(makeType, CustomizeIndex.SkinColor));
		menus.Add(this.GetMenu(makeType, CustomizeIndex.LipColor)); // Hroth fur pattern

		await this.dispatcher.MainThread();

		this.BodyMenus.Replace(menus);
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