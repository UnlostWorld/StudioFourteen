// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData.Excel;

using ScreenshotStudio.Tags;

public enum Genders
{
	Masculine,
	Feminine,
}

public static class GendersExtensions
{
	public static TagCollection ToTags(this Genders self)
	{
		TagCollection tags = new();
		tags.Add(self.ToString());
		return tags;
	}
}