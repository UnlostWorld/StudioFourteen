namespace ScreenshotStudio.GameData;

using Lumina.Excel;
using ScreenshotStudio.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ScreenshotStudio.GameData.Excel;
using ScreenshotStudio.Plugin;
using Lumina.Data;
using ScreenshotStudio.GameData.Sheets;
using System.IO;

public class GameDataService : ServiceBase
{
	private readonly Dictionary<Type, DataSheet> sheets = new();

	public static ItemsSheet? Items => Get<Item>() as ItemsSheet;
	public static BuddyEquipsSheet? BuddyEquips => Get<BuddyEquip>() as BuddyEquipsSheet;

	public static T? GetFile<T>(string path)
		where T : FileResource
	{
		if (ServiceManager.ShutdownRequested)
			return null;

		string? newPath = DalamudServices.TextureSubstitutionProvider?.GetSubstitutedPath(path);

		if (newPath == null)
			newPath = path;

		if (Path.IsPathRooted(newPath))
		{
			return DalamudServices.DataManager?.GameData.GetFileFromDisk<T>(newPath);
		}
		else
		{
			return DalamudServices.DataManager?.GetFile<T>(newPath);
		}
	}

	public static DataSheet<T>? Get<T>()
		where T : ExcelRow
	{
		return ServiceManager.Instance.Data.GetSheet<T>();
	}

	public static T? GetRow<T>(uint row)
		where T : ExcelRow
	{
		return Get<T>()?.GetRow(row);
	}

	public static T? GetRow<T>(byte row)
		where T : ExcelRow
	{
		return Get<T>()?.GetRow(row);
	}

	public static T? GetRow<T>(int row)
		where T : ExcelRow
	{
		return Get<T>()?.GetRow(row);
	}

	public DataSheet<T>? GetSheet<T>()
		where T : ExcelRow
	{
		Type type = typeof(T);
		if (!this.sheets.ContainsKey(type))
		{
			this.Log.Error($"No sheet for row type: {type}");
			return null;
		}

		return this.sheets[type] as DataSheet<T>;
	}

	public override async Task Initialize()
	{
		await base.Initialize();

		// Add sheets here
		this.AddSheet(new ItemsSheet());
		this.AddSheet(new BuddyEquipsSheet());
		this.AddSheet(new EventNpcSheet());
		this.AddSheet(new ResidentNpcSheet());
		this.AddSheet(new TerritoryTypeSheet());

		this.AddSheet<Race>();
		this.AddSheet<Tribe>();
		this.AddSheet<BattleNpc>();
		this.AddSheet<BattleNpcCustomize>();
		this.AddSheet<BattleNpcName>();
		this.AddSheet<CharaMakeCustomize>();
		this.AddSheet<HairMakeType>();
		this.AddSheet<CharaMakeType>();
		this.AddSheet<ClassJobCategory>();
		this.AddSheet<Companion>();
		this.AddSheet<EquipRaceCategory>();
		this.AddSheet<EquipSlotCategory>();
		this.AddSheet<Lobby>();
		this.AddSheet<ModelChara>();
		this.AddSheet<Mount>();
		this.AddSheet<MountCustomize>();
		this.AddSheet<NpcEquip>();
		this.AddSheet<Ornament>();
		this.AddSheet<Perform>();
		this.AddSheet<Stain>();
		this.AddSheet<Weather>();
		this.AddSheet<WeatherRate>();
		this.AddSheet<ClassJob>();
		this.AddSheet<ItemUICategory>();

		this.AddSheet<Lumina.Excel.GeneratedSheets.PlaceName>();

		// Initialize all sheets
		// TODO: possibly do this in parallel
		foreach (DataSheet sheet in this.sheets.Values)
		{
			await sheet.Initialize();
		}
	}

	public override async Task Shutdown()
	{
		await base.Shutdown();

		foreach (DataSheet sheet in this.sheets.Values)
		{
			await sheet.Shutdown();
		}

		this.sheets.Clear();
	}

	private void AddSheet<T>()
		where T : ExcelRow
	{
		this.AddSheet(new DataSheet<T>());
	}

	private void AddSheet(DataSheet sheet)
	{
		if (this.sheets.ContainsKey(sheet.RowType))
		{
			this.Log.Error($"Duplicate data sheet: {sheet.RowType}");
			return;
		}

		this.sheets.Add(sheet.RowType, sheet);
	}
}