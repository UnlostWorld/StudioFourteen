namespace ScreenshotStudio.GameData.Sheets;

using ScreenshotStudio.GameData.Excel;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

public class EventNpcSheet : DataSheet<EventNpc>
{
	private readonly List<uint> uniqueAppearances = new();

	public override uint Count => (uint)this.uniqueAppearances.Count;

	public override async Task Initialize()
	{
		await base.Initialize();

		if (this.Sheet == null)
			return;

		Stopwatch sw = new();
		sw.Start();

		Dictionary<string, uint> hashes = new();
		foreach (EventNpc eventNpc in this.Sheet)
		{
			if (eventNpc.AppearanceHash == null)
			{
				this.Log.Warning($"{eventNpc.RowName} has no appearance hash");
				continue;
			}

			if(hashes.TryGetValue(eventNpc.AppearanceHash, out uint orignalRowId))
			{
				eventNpc.DuplicateRow = orignalRowId;
			}
			else
			{
				hashes.Add(eventNpc.AppearanceHash, eventNpc.RowId);
				eventNpc.DuplicateRow = null;
				this.uniqueAppearances.Add(eventNpc.RowId);
			}
		}

		sw.Stop();
		this.Log.Information($"Found {hashes.Count} unique event NPC appearances in {this.Sheet.RowCount} event NPCs in {sw.ElapsedMilliseconds}ms");
	}

	public override IEnumerator GetEnumerator()
	{
		foreach (uint rowId in this.uniqueAppearances)
		{
			yield return this.GetRow(rowId);
		}
	}

	public override Task Shutdown()
	{
		this.uniqueAppearances.Clear();
		return base.Shutdown();
	}
}
