//// Ktisis
//// https://github.com/ktisis-tools/Ktisis/
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Data/Excel/Item.cs

//// Anamnesis
//// https://github.com/imchillin/Anamnesis/blob/master/Anamnesis/Actor/Utilities/ItemUtility.cs

namespace ScreenshotStudio.GameData.Sheets;

using ScreenshotStudio.Services;
using ScreenshotStudio.Structs;
using ScreenshotStudio.GameData.Excel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;

public class ItemsSheet : DataSheet<Item>
{
	public static readonly DummyItem None = new DummyItem(0, 0, 0, string.Empty);

	private readonly ConcurrentDictionary<string, uint> itemCache = new();

	public Item? Find(EquipmentSlot slot, ushort modelSet, ushort modelBase, ushort modelVariant)
	{
		if (this.Sheet == null)
			return null;

		if (modelSet == 0 && modelBase == 0 && modelVariant == 0)
			return None;

		string lookupKey = slot + "_" + modelSet + "_" + modelBase + "_" + modelVariant;
		this.itemCache.TryGetValue(lookupKey, out uint itemRow);

		if (itemRow == 0)
		{
			return new DummyItem(modelSet, modelBase, modelVariant);
		}

		return this.GetRow(itemRow);
	}

	public Item? Find(EquipmentSlot slot, ulong val)
	{
		if (val == 0)
			return None;

		if (val == uint.MaxValue || val == long.MaxValue || val == ulong.MaxValue)
			return null;

		short modelSet = 0;
		short modelBase = (short)val;
		short modelVariant = (short)(val >> 16);

		if (modelSet < 0 || modelBase < 0 || modelVariant < 0)
		{
			this.Log.Warning($"Invalid item value: {val}");
			return null;
		}

		return this.Find(slot, (ushort)modelSet, (ushort)modelBase, (ushort)modelVariant);
	}

	public Item? Find(WeaponSlot slot, ushort modelSet, ushort modelBase, ushort modelVariant)
	{
		if (this.Sheet == null)
			return null;

		if (modelSet == 0 && modelBase == 0 && modelVariant == 0)
			return None;

		string lookupKey = slot + "_" + modelSet + "_" + modelBase + "_" + modelVariant;
		this.itemCache.TryGetValue(lookupKey, out uint itemRow);

		if (itemRow == 0)
		{
			return new DummyItem(modelSet, modelBase, modelVariant);
		}

		return this.GetRow(itemRow);
	}

	public Item? Find(WeaponSlot slot, ulong val)
	{
		if (val == 0)
			return None;

		if (val == uint.MaxValue || val == long.MaxValue || val == ulong.MaxValue)
			return null;

		short modelSet = (short)val;
		short modelBase = (short)(val >> 16);
		short modelVariant = (short)(val >> 32);

		if (modelSet < 0 || modelBase < 0 || modelVariant < 0)
		{
			this.Log.Warning($"Invalid item value: {val}");
			return null;
		}

		return this.Find(slot, (ushort)modelSet, (ushort)modelBase, (ushort)modelVariant);
	}

	public override async Task Initialize()
	{
		await base.Initialize();

		Stopwatch sw = new();
		sw.Start();

		List<Task> tasks = new();
		foreach (EquipmentSlot slot in Enum.GetValues<EquipmentSlot>())
		{
			tasks.Add(Task.Run(() => this.PopulateCache(slot)));
		}

		foreach (WeaponSlot slot in Enum.GetValues<WeaponSlot>())
		{
			tasks.Add(Task.Run(() => this.PopulateCache(slot)));
		}

		await Task.WhenAll(tasks);
		this.Log.Information($"Cached {this.itemCache.Count} item models in {sw.ElapsedMilliseconds}ms");
	}

	public override Task Shutdown()
	{
		this.itemCache.Clear();
		return base.Shutdown();
	}

	private void PopulateCache(WeaponSlot slot)
	{
		if (this.Sheet == null)
			return;

		foreach (Item tItem in this.Sheet)
		{
			// Abort
			if (!ServiceManager.Instance.GameData.IsAlive)
				return;

			try
			{
				if (!tItem.FitsInSlot(slot))
					continue;

				string lookupKey = $"{slot}_{tItem.ModelSet}_{tItem.ModelBase}_{tItem.ModelVariant}";
				if (!this.itemCache.ContainsKey(lookupKey))
				{
					this.itemCache.TryAdd(lookupKey, tItem.RowId);
				}

				if (tItem.HasSubModel)
				{
					lookupKey = $"{slot}_{tItem.SubModelSet}_{tItem.SubModelBase}_{tItem.SubModelVariant}";
					if (!this.itemCache.ContainsKey(lookupKey))
					{
						this.itemCache.TryAdd(lookupKey, tItem.RowId);
					}
				}
			}
			catch (Exception ex)
			{
				this.Log.Error(ex, $"Error caching {tItem}");
			}
		}
	}

	private void PopulateCache(EquipmentSlot slot)
	{
		if (this.Sheet == null)
			return;

		foreach (Item tItem in this.Sheet)
		{
			// Abort
			if (!ServiceManager.Instance.GameData.IsAlive)
				return;

			try
			{
				// Big old hack, but we prefer the emperors bracelets to the promise bracelets (even though they are the same model)
				if (slot == EquipmentSlot.Wrists && tItem.Name != null && tItem.Name.StartsWith("Promise of"))
					continue;

				if (!tItem.FitsInSlot(slot))
					continue;

				string lookupKey = $"{slot}_{tItem.ModelSet}_{tItem.ModelBase}_{tItem.ModelVariant}";
				if (!this.itemCache.ContainsKey(lookupKey))
				{
					this.itemCache.TryAdd(lookupKey, tItem.RowId);
				}

				if (tItem.HasSubModel)
				{
					lookupKey = $"{slot}_{tItem.SubModelSet}_{tItem.SubModelBase}_{tItem.SubModelVariant}";
					if (!this.itemCache.ContainsKey(lookupKey))
					{
						this.itemCache.TryAdd(lookupKey, tItem.RowId);
					}
				}
			}
			catch (Exception ex)
			{
				this.Log.Error(ex, $"Error caching {tItem}");
			}
		}
	}
}

public class DummyItem : Item
{
	public DummyItem(ushort modelSet, ushort modelBase, ushort modelVariant, string name = "???")
	{
		this.ModelSet = modelSet;
		this.ModelBase = modelBase;
		this.ModelVariant = modelVariant;
		this.Name = name;
	}

	public DummyItem(string name = "???")
	{
		this.Name = name;
	}
}