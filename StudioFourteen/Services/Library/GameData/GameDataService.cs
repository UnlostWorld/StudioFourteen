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

namespace StudioFourteen.Services.Library.GameData;

using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using StudioFourteen.Services.Library.GameData.Library;
using System;
using System.Collections.Generic;
using System.IO;

public class GameDataLibrary
{
	public readonly Dictionary<string, uint> BattleNpcNameIndex = new();
	private readonly Dictionary<Type, ExcelSheetLibrarySource> librarySourceLookup = new();
	private readonly LibraryService library;

	public GameDataLibrary(LibraryService libraryService)
	{
		this.library = libraryService;

		////OnlineJsonFile<Dictionary<string, int>> bNpcNameIndexFile = new("https://raw.githubusercontent.com/ffxiv-teamcraft/ffxiv-teamcraft/refs/heads/staging/libs/data/src/lib/json/gubal-bnpcs-index.json", 1);
		////BattleNpcNameIndex = await bNpcNameIndexFile.GetAsync();

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
		////this.AddLibraryExcelSheet<Lumina.Excel.Sheets.Action, ActionLibraryEntry>();
		this.AddLibraryExcelSheet<Mount, MountLibraryEntry>();
		this.AddLibraryExcelSheet<Status, StatusLibraryEntry>();

		this.library.AddSource(new HairLibrarySource());
		this.library.AddSource(new FacePaintLibrarySource());
	}

	public ItemLibrarySource? Items { get; private set; }
	public BNpcBaseLibrarySource? BNpcBase { get; private set; }
	public ENpcResidentLibrarySource? ENpcResidents { get; private set; }

	public T? GetFile<T>(string path)
		where T : FileResource
	{
		if (Studio.IsDisposed)
			return null;

		string? newPath = Studio.TextureSubstitutionProvider.GetSubstitutedPath(path) ?? path;
		if (Path.IsPathRooted(newPath))
		{
			return Studio.DataManager.GameData.GetFileFromDisk<T>(newPath);
		}
		else
		{
			return Studio.DataManager.GetFile<T>(newPath);
		}
	}

	public bool GetFileExists(string path)
	{
		if (Studio.IsDisposed)
			return false;

		string? newPath = Studio.TextureSubstitutionProvider.GetSubstitutedPath(path) ?? path;
		if (Path.IsPathRooted(newPath))
		{
			// TODO
			throw new NotSupportedException();
		}
		else
		{
			return Studio.DataManager.FileExists(newPath);
		}
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
		this.library.AddSource(source);
	}
}