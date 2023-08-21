// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData;

using ScreenshotStudio.Services;
using System.Threading.Tasks;

public class GameDataService : ServiceBase
{
	public ItemsSheet Items { get; init; } = new();
	public EquipSlotCategorySheet EquipSlotCategories { get; init; } = new();

	public override async Task Initialize()
	{
		await base.Initialize();
		await this.Items.PopulateCache();
	}
}