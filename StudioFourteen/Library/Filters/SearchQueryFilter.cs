namespace StudioFourteen.Library.Filters;

using WpfUtils;

public class SearchQueryFilter : FilterBase
{
	public string[]? Query;

	private string? search;

	public string? Search
	{
		get => this.search;
		set
		{
			this.search = value;

			if (string.IsNullOrWhiteSpace(value))
			{
				this.Query = null;
			}
			else
			{
				this.Query = SearchUtility.ToQuery(value);
			}
		}
	}

	public override bool IsEmpty => this.Query == null;

	public override void Clear()
	{
		this.Query = null;
		this.search = null;
	}

	public override bool Filter(LibraryEntryBase entry)
	{
		if(this.Query == null)
			return true;

		return entry.Search(this.Query);
	}
}
