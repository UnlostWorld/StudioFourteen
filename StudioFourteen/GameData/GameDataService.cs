namespace StudioFourteen.GameData;

using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.Exceptions;
using StudioFourteen.GameData.Excel;
using StudioFourteen.GameData.Sheets;
using StudioFourteen.Plugin;
using StudioFourteen.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

public class GameDataService : ServiceBase
{
	public static Lumina.GameData? DataProvider;

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
		return ServiceManager.Instance.GameData.GetSheet<T>();
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

	public ExcelSheet<T>? GetLuminaExcelSheet<T>()
		where T : ExcelRow
	{
		ExcelSheet<T>? sheet;
		sheet = DalamudServices.DataManager?.GetExcelSheet<T>();

		if (DataProvider != null)
			sheet = DataProvider.GetExcelSheet<T>();

		if (sheet == null)
		{
			this.Log.Error($"Failed to get excel sheet for type: {typeof(T)}");
		}

		return sheet;
	}

	public override async Task Initialize()
	{
		await base.Initialize();

		// Add sheets here
		this.AddSheet(new ItemsSheet());
		this.AddSheet(new BuddyEquipsSheet());
		this.AddSheet(new TerritoryTypeSheet());

		this.AddSheet<Race>();
		this.AddSheet<Tribe>();
		this.AddSheet<CharaMakeCustomize>();
		this.AddSheet<HairMakeType>();
		this.AddSheet<CharaMakeType>();
		this.AddSheet<ClassJobCategory>();
		this.AddSheet<ModelChara>();
		this.AddSheet<EventNpc>();
		this.AddSheet<ResidentNpc>();
		this.AddSheet<BattleNpc>();
		this.AddSheet<BattleNpcCustomize>();
		this.AddSheet<BattleNpcName>();
		this.AddSheet<Companion>();
		this.AddSheet<EquipRaceCategory>();
		this.AddSheet<EquipSlotCategory>();
		this.AddSheet<Lobby>();
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
		this.AddSheet<Glasses>();

		this.AddSheet<Lumina.Excel.GeneratedSheets.PlaceName>();

		// Initialize all sheets
		// TODO: possibly do this in parallel
		/*foreach (DataSheet sheet in this.sheets.Values)
		{
			await sheet.Initialize();
		}*/

		NameMergeUtil.MergeNpcNames();
		AppearanceDeduplicationUtil.Deduplicate();
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
		try
		{
			this.AddSheet(new DataSheet<T>());
		}
		catch (ExcelSheetColumnChecksumMismatchException ex)
		{
			this.Log.Error(ex, $"Excel Column checksum mismatch for sheet type {typeof(T)}");
		}
		catch(Exception)
		{
		}
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

	// updates the name of any unnamed npc that has a matching appearance that is named.
	public static class NameMergeUtil
	{
		public static ILogger Log => Logging.ForContext("NameMergeUtil");

		public static void MergeNpcNames()
		{
			Stopwatch sw = new();
			sw.Start();

			Dictionary<string, string> hashToName = new();

			DataSheet? eventNpcSheet = GameDataService.Get<EventNpc>();
			DataSheet? battleNpcSheet = GameDataService.Get<BattleNpc>();

			int count = 0;

			if (eventNpcSheet != null && battleNpcSheet != null)
			{
				StoreNames(eventNpcSheet, ref hashToName);
				StoreNames(battleNpcSheet, ref hashToName);

				UpdateNames(eventNpcSheet, ref hashToName, ref count);
				UpdateNames(battleNpcSheet, ref hashToName, ref count);
			}

			sw.Stop();
			Log.Information($"Merged {count} NPC names in {sw.ElapsedMilliseconds}ms");
		}

		private static void StoreNames(DataSheet sheet, ref Dictionary<string, string> hashToName)
		{
			foreach (NpcBase npc in sheet)
			{
				if (npc.AppearanceHash != null && npc.Name != null)
				{
					hashToName.TryAdd(npc.AppearanceHash, npc.Name);
				}
			}
		}

		private static void UpdateNames(DataSheet sheet, ref Dictionary<string, string> hashToName, ref int count)
		{
			foreach (NpcBase npc in sheet)
			{
				if (npc.AppearanceHash != null)
				{
					if (hashToName.TryGetValue(npc.AppearanceHash, out string? newName) && newName != npc.Name)
					{
						npc.Name = hashToName[npc.AppearanceHash];
						count++;
					}
				}
			}
		}
	}

	public static class AppearanceDeduplicationUtil
	{
		public static ILogger Log => Logging.ForContext("AppearanceDeduplicationUtil");

		public static void Deduplicate()
		{
			Stopwatch sw = new();
			sw.Start();
			int count = 0;

			DataSheet? eventNpcSheet = GameDataService.Get<EventNpc>();
			if (eventNpcSheet != null)
				Deduplicate(eventNpcSheet, ref count);

			DataSheet? battleNpcSheet = GameDataService.Get<BattleNpc>();
			if (battleNpcSheet != null)
				Deduplicate(battleNpcSheet, ref count);

			sw.Stop();
			Log.Information($"Found {count} duplicate appearances in {sw.ElapsedMilliseconds}ms");
		}

		private static void Deduplicate(DataSheet sheet, ref int count)
		{
			Dictionary<string, uint> hashes = new();
			foreach (NpcBase npc in sheet)
			{
				if (npc.AppearanceHash == null)
				{
					Log.Warning($"{npc.RowName} has no appearance hash");
					continue;
				}

				if (hashes.TryGetValue(npc.AppearanceHash, out uint orignalRowId))
				{
					npc.DuplicateRow = orignalRowId;
					count++;
				}
				else
				{
					hashes.Add(npc.AppearanceHash, npc.RowId);
					npc.DuplicateRow = null;
				}
			}
		}
	}
}