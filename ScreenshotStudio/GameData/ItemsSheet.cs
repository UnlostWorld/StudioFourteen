// © XivTools.
// Licensed under the MIT license.

//// Ktisis
//// https://github.com/ktisis-tools/Ktisis/
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Data/Excel/Item.cs

//// Anamnesis
//// https://github.com/imchillin/Anamnesis/blob/master/Anamnesis/Actor/Utilities/ItemUtility.cs

namespace ScreenshotStudio.GameData;

using Lumina;
using Lumina.Data;
using Lumina.Excel;
using ScreenshotStudio.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

public class ItemsSheet : DataSheet<Item>
{
	private readonly ConcurrentDictionary<string, uint> itemCache = new();

	public Item? Find(EquipSlots slot, ushort modelSet, ushort modelBase, ushort modelVariant, bool isChocobo)
	{
		if (this.Sheet == null)
			return null;

		string lookupKey = slot + "_" + modelSet + "_" + modelBase + "_" + modelVariant;
		this.itemCache.TryGetValue(lookupKey, out uint itemRow);
		return this.GetRow(itemRow);
	}

	public async Task PopulateCache()
	{
		Stopwatch sw = new();
		sw.Start();

		List<Task> tasks = new();
		for(int i = 0; i < (int)EquipSlots.SoulCrystal; i++)
		{
			EquipSlots slot = (EquipSlots)i;
			tasks.Add(Task.Run(() => this.PopulateCache(slot)));
		}

		await Task.WhenAll(tasks);
		this.Log.Information($"Cached {this.itemCache.Count} item models in {sw.ElapsedMilliseconds}ms");
	}

	private void PopulateCache(EquipSlots slot)
	{
		if (this.Sheet == null)
			return;

		int count = 0;
		foreach (Item tItem in this.Sheet)
		{
			// Abort
			if (!ServiceManager.Instance.Data.IsAlive)
				return;

			try
			{
				// Big old hack, but we prefer the emperors bracelets to the promise bracelets (even though they are the same model)
				if (slot == EquipSlots.Bracelet && tItem.DisplayName.StartsWith("Promise of"))
					continue;

				if (!tItem.CanEquipToSlot(slot))
					continue;

				string lookupKey = slot + "_" + tItem.ModelMainSet + "_" + tItem.ModelMainBase + "_" + tItem.ModelMainVariant;
				count++;

				if (!this.itemCache.ContainsKey(lookupKey))
				{
					this.itemCache.TryAdd(lookupKey, tItem.RowId);
				}

				if (tItem.HasSubModel)
				{
					lookupKey = slot + "_" + tItem.ModelSubSet + "_" + tItem.ModelSubBase + "_" + tItem.ModelSubVariant;

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

public class Item : Lumina.Excel.GeneratedSheets.Item
{
	private byte equipSlotCategoryRow;

	public new ImageReference? Icon { get; private set; }

	public ushort ModelMainSet { get; private set; }
	public ushort ModelMainBase { get; private set; }
	public ushort ModelMainVariant { get; private set; }

	public ushort ModelSubSet { get; private set; }
	public ushort ModelSubBase { get; private set; }
	public ushort ModelSubVariant { get; private set; }

	public new EquipSlotCategory? EquipSlotCategory => ServiceManager.Instance.Data.EquipSlotCategories.GetRow(this.equipSlotCategoryRow);
	public bool IsWeapon { get; private set; }
	public bool IsEquippable => this.equipSlotCategoryRow != 0;
	public bool HasSubModel => this.ModelSubSet != 0;

	public string DisplayName => this.Name.RawString;

	public override void PopulateData(RowParser parser, GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);
		this.equipSlotCategoryRow = parser.ReadColumn<byte>(17);
		this.Icon = new(base.Icon);

		this.IsWeapon = this.CanEquipToSlot(EquipSlots.MainHand) || this.CanEquipToSlot(EquipSlots.OffHand);

		if (this.IsWeapon)
		{
			this.ModelMainSet = (ushort)this.ModelMain;
			this.ModelMainBase = (ushort)(this.ModelMain >> 16);
			this.ModelMainVariant = (ushort)(this.ModelMain >> 32);

			this.ModelSubSet = (ushort)this.ModelSub;
			this.ModelSubBase = (ushort)(this.ModelMain >> 16);
			this.ModelSubVariant = (ushort)(this.ModelMain >> 32);
		}
		else
		{
			this.ModelMainSet = 0;
			this.ModelMainBase = (ushort)this.ModelMain;
			this.ModelMainVariant = (ushort)(this.ModelMain >> 16);

			this.ModelSubSet = 0;
			this.ModelSubBase = (ushort)this.ModelSub;
			this.ModelSubVariant = (ushort)(this.ModelSub >> 16);
		}
	}

	public bool CanEquipToSlot(EquipSlots slot) => this.IsEquippable && (this.EquipSlotCategory?.IsEquippable(slot) ?? false);
}