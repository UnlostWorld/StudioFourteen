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

namespace StudioFourteen.GameData;

using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using StudioFourteen.GameData.Library;
using StudioFourteen.Online;
using StudioFourteen.Plugin;
using StudioFourteen.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

public class GameDataService : ServiceBase
{
	public static Dictionary<string, int> BattleNpcNameIndex = new();
	private readonly Dictionary<Type, ExcelSheetLibrarySource> librarySourceLookup = new();
	private Lumina.GameData? lumina;

	public ItemLibrarySource? Items { get; private set; }
	public BNpcBaseLibrarySource? BNpcBase { get; private set; }
	public ENpcResidentLibrarySource? ENpcResidents { get; private set; }

	public T? GetFile<T>(string path)
		where T : FileResource
	{
		if (ServiceManager.ShutdownRequested)
			return null;

		if (this.lumina == null)
			return null;

		string? newPath = DalamudServices.TextureSubstitutionProvider?.GetSubstitutedPath(path) ?? path;
		if (Path.IsPathRooted(newPath))
		{
			return this.lumina.GetFileFromDisk<T>(newPath);
		}
		else
		{
			return this.lumina.GetFile<T>(newPath);
		}
	}

	public ExcelSheet<T>? GetSheet<T>()
		where T : struct, IExcelRow<T>
	{
		if (this.lumina == null)
			return null;

		ExcelSheet<T>? sheet;
		sheet = this.lumina.GetExcelSheet<T>();

		if (sheet == null)
			this.Log.Error($"Failed to get excel sheet for type: {typeof(T)}");

		return sheet;
	}

	public T? GetRow<T>(int rowIndex)
		where T : struct, IExcelRow<T>
	{
		return this.GetRow<T>((uint)rowIndex);
	}

	public T? GetRow<T>(uint rowIndex)
		where T : struct, IExcelRow<T>
	{
		ExcelSheet<T>? sheet;
		sheet = this.GetSheet<T>();

		if (sheet == null)
			return null;

		return sheet.GetRowOrDefault(rowIndex);
	}

	public ExcelSheetLibrarySource<TEntry>? GetLibrarySource<TEntry>()
		where TEntry : ExcelLibraryEntry
	{
		Type entryType = typeof(TEntry);
		ExcelSheetLibrarySource? source;
		if (!this.librarySourceLookup.TryGetValue(entryType, out source))
			return null;

		return source as ExcelSheetLibrarySource<TEntry>;
	}

	public TEntry? GetLibraryEntry<TEntry>(uint rowId)
		where TEntry : ExcelLibraryEntry
	{
		ExcelSheetLibrarySource? source = this.GetLibrarySource<TEntry>();
		return source?.GetRowObject(rowId) as TEntry;
	}

	public override async Task Initialize()
	{
		await base.Initialize();

		this.lumina = DalamudServices.DataManager?.GameData;

#if DEBUG
		if (this.lumina == null)
		{
			string? dir;
			if (this.Services.Windows.XivProcess == null)
			{
				dir = "C:/Program Files (x86)/Steam/steamapps/common/FINAL FANTASY XIV Online/game/sqpack/";
			}
			else
			{
				dir = $"{Path.GetDirectoryName(this.Services.Windows.XivProcess?.MainModule?.FileName)}/sqpack/";
			}

			this.lumina = new Lumina.GameData(dir);
		}
#endif

		OnlineJsonFile<Dictionary<string, int>> bNpcNameIndexFile = new("https://raw.githubusercontent.com/ffxiv-teamcraft/ffxiv-teamcraft/refs/heads/staging/libs/data/src/lib/json/gubal-bnpcs-index.json", 1);
		BattleNpcNameIndex = await bNpcNameIndexFile.GetAsync();

		this.Items = new();
		this.AddLibraryExcelSheet<ItemLibraryEntry>(this.Items);

		this.BNpcBase = new();
		this.AddLibraryExcelSheet<BNpcBaseLibraryEntry>(this.BNpcBase);

		this.ENpcResidents = new();
		this.AddLibraryExcelSheet<ENpcResidentLibraryEntry>(this.ENpcResidents);

		this.AddLibraryExcelSheet<Race, RaceLibraryEntry>();
		this.AddLibraryExcelSheet<Tribe, TribeLibraryEntry>();
		this.AddLibraryExcelSheet<Glasses, GlassesLibraryEntry>();
		this.AddLibraryExcelSheet<Ornament, OrnamentLibraryEntry>();
		this.AddLibraryExcelSheet<Stain, StainLibraryEntry>();
		this.AddLibraryExcelSheet<TerritoryType, TerritoryTypeLibraryEntry>();
		this.AddLibraryExcelSheet<Weather, WeatherLibraryEntry>();
		this.AddLibraryExcelSheet<Emote, EmoteLibraryEntry>();
		this.AddLibraryExcelSheet<Lumina.Excel.Sheets.Action, ActionLibraryEntry>();
		this.AddLibraryExcelSheet<Mount, MountLibraryEntry>();

		this.Services.Library.AddSource(new HairLibrarySource());
		this.Services.Library.AddSource(new FacePaintLibrarySource());
	}

	private ExcelSheetLibrarySource AddLibraryExcelSheet<TExcel, TEntry>()
		where TExcel : struct, IExcelRow<TExcel>
		where TEntry : ExcelLibraryEntry
	{
		ExcelSheetLibrarySource<TExcel, TEntry> source = new();
		this.AddLibraryExcelSheet<TEntry>(source);
		return source;
	}

	private void AddLibraryExcelSheet<TEntry>(ExcelSheetLibrarySource source)
		where TEntry : ExcelLibraryEntry
	{
		Type entryType = typeof(TEntry);

		if (this.librarySourceLookup.ContainsKey(typeof(TEntry)))
			throw new NotSupportedException("Attempt to add duplicate excel type to library sources");

		this.librarySourceLookup.Add(typeof(TEntry), source);
		this.Services.Library.AddSource(source);
	}
}