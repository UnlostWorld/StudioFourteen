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

namespace StudioFourteen.Services.Library.GameData.Library;

using System.Collections.Generic;
using System.Diagnostics;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using global::Dalamud.Plugin.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

using ENpcBase = StudioFourteen.Services.Library.GameData.Sheets.ENpcBase;

public class ENpcResidentLibraryEntry : ExcelLibraryEntry
{
	public readonly ENpcResident Npc;

	public ENpcResidentLibraryEntry(SourceBase source, ENpcResident npc)
	: base(source, npc.RowId)
	{
		this.Npc = npc;
	}

	public override string? Name => this.Npc.Singular.GetString();

	public override object? Icon
	{
		get
		{
			CustomizeData? customize = this.ENpcBase?.CustomizeData;
			if (customize == null)
				return null;

			CustomizeData d = customize.Value;
			return d.GetIcon();
		}
	}

	public ENpcBase? ENpcBase => Studio.DataManager.GetRow<ENpcBase>(this.Npc.RowId);
}

public class ENpcResidentLibrarySource : ExcelSheetLibrarySource<ENpcResident, ENpcResidentLibraryEntry>
{
	private readonly Dictionary<uint, uint> duplicateRowMap = new();

	public bool IsDuplicate(ENpcResident npcResident)
	{
		return this.duplicateRowMap.ContainsKey(npcResident.RowId);
	}

	protected override void Scan()
	{
		this.Deduplicate();
		base.Scan();
	}

	protected override bool IncludeEntry(ENpcResident row)
	{
		if (this.duplicateRowMap.ContainsKey(row.RowId))
			return false;

		return base.IncludeEntry(row);
	}

	private void Deduplicate()
	{
		this.duplicateRowMap.Clear();

		Stopwatch sw = new();
		sw.Start();
		int count = 0;

		ExcelSheet<ENpcResident>? npcResidentSheet = Studio.DataManager.GetExcelSheet<ENpcResident>();
		if (npcResidentSheet != null)
			this.Deduplicate(npcResidentSheet, ref count);

		sw.Stop();
		Studio.Log.Information($"Found {count} duplicate appearances in {sw.ElapsedMilliseconds}ms");
	}

	private void Deduplicate(ExcelSheet<ENpcResident> sheet, ref int count)
	{
		Dictionary<string, uint> hashToRowMap = new();
		foreach (ENpcResident npc in sheet)
		{
			string hash = npc.Singular.GetString() + npc.GetAppearanceHash();

			if (hashToRowMap.TryGetValue(hash, out uint originalRowId))
			{
				this.duplicateRowMap.Add(npc.RowId, originalRowId);
				count++;
			}
			else
			{
				hashToRowMap.Add(hash, npc.RowId);
			}
		}
	}
}