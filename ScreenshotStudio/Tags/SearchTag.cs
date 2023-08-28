// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Tags;

using FontAwesome.Sharp.Pro;

public class SearchTag : Tag
{
	public SearchTag()
		: base(string.Empty)
	{
	}

	public SearchTag(string search)
		: base(search)
	{
	}

	public string? Query
	{
		get => this.Name;
		set
		{
			this.Name = value;
			this.NotifyPropertyChanged();
		}
	}

	public override ProIcons Icon => ProIcons.Search;
}
