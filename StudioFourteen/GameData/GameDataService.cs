namespace StudioFourteen.GameData;

using Lumina.Data;
using Lumina.Excel;
using Serilog;
using StudioFourteen.GameData.Excel;
using StudioFourteen.GameData.Sheets;
using StudioFourteen.Online;
using StudioFourteen.Plugin;
using StudioFourteen.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

public class GameDataService : ServiceBase
{
	public static Dictionary<string, int> BattleNpcNameIndex = new();

	public readonly ItemUtility Items = new();

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

	public ExcelSheet<T>? GetSheet<T>()
		where T : ExcelRow
	{
		ExcelSheet<T>? sheet;
		sheet = DalamudServices.DataManager?.GetExcelSheet<T>();

		if (sheet == null)
			this.Log.Error($"Failed to get excel sheet for type: {typeof(T)}");

		return sheet;
	}

	public T? GetRow<T>(int rowIndex)
		where T : ExcelRow
	{
		return this.GetRow<T>((uint)rowIndex);
	}

	public T? GetRow<T>(uint rowIndex)
		where T : ExcelRow
	{
		ExcelSheet<T>? sheet;
		sheet = this.GetSheet<T>();

		if (sheet == null)
			return null;

		return sheet.GetRow(rowIndex);
	}

	public override async Task Initialize()
	{
		await base.Initialize();

		OnlineJsonFile<Dictionary<string, int>> bNpcNameIndexFile = new("https://raw.githubusercontent.com/ffxiv-teamcraft/ffxiv-teamcraft/refs/heads/staging/libs/data/src/lib/json/gubal-bnpcs-index.json", 1);
		BattleNpcNameIndex = await bNpcNameIndexFile.GetAsync();

		_ = Task.Run(() => NameMergeUtil.MergeNpcNames());
		_ = Task.Run(() => AppearanceDeduplicationUtil.Deduplicate());

		this.Services.Library.AddSource(new ExcelSheetLibrarySource<Item>());
		this.Services.Library.AddSource(new ExcelSheetLibrarySource<BattleNpc>());
		this.Services.Library.AddSource(new ExcelSheetLibrarySource<ResidentNpc>());
		this.Services.Library.AddSource(new ExcelSheetLibrarySource<Territory>());
		this.Services.Library.AddSource(new ExcelSheetLibrarySource<Weather>());
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

			ExcelSheet<EventNpc>? eventNpcSheet = ServiceManager.Instance.GameData.GetSheet<EventNpc>();
			ExcelSheet<BattleNpc>? battleNpcSheet = ServiceManager.Instance.GameData.GetSheet<BattleNpc>();

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

		private static void StoreNames(ExcelSheet<EventNpc> sheet, ref Dictionary<string, string> hashToName)
		{
			foreach (NpcBase npc in sheet)
			{
				if (npc.AppearanceHash != null && npc.Name != null)
				{
					hashToName.TryAdd(npc.AppearanceHash, npc.Name);
				}
			}
		}

		private static void StoreNames(ExcelSheet<BattleNpc> sheet, ref Dictionary<string, string> hashToName)
		{
			foreach (NpcBase npc in sheet)
			{
				if (npc.AppearanceHash != null && npc.Name != null)
				{
					hashToName.TryAdd(npc.AppearanceHash, npc.Name);
				}
			}
		}

		private static void UpdateNames(ExcelSheet<EventNpc> sheet, ref Dictionary<string, string> hashToName, ref int count)
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

		private static void UpdateNames(ExcelSheet<BattleNpc> sheet, ref Dictionary<string, string> hashToName, ref int count)
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

			ExcelSheet<EventNpc>? eventNpcSheet = ServiceManager.Instance.GameData.GetSheet<EventNpc>();
			if (eventNpcSheet != null)
				Deduplicate(eventNpcSheet, ref count);

			ExcelSheet<BattleNpc>? battleNpcSheet = ServiceManager.Instance.GameData.GetSheet<BattleNpc>();
			if (battleNpcSheet != null)
				Deduplicate(battleNpcSheet, ref count);

			sw.Stop();
			Log.Information($"Found {count} duplicate appearances in {sw.ElapsedMilliseconds}ms");
		}

		private static void Deduplicate(ExcelSheet<EventNpc> sheet, ref int count)
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

		private static void Deduplicate(ExcelSheet<BattleNpc> sheet, ref int count)
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