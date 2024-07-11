namespace ScreenshotStudio.Library.Filters;

using ScreenshotStudio.Tags;

public class TagFilter : FilterBase
{
	public TagCollection Tags { get; init; } = new();

	public override bool IsEmpty => this.Tags.Count == 0;

	public override void Clear()
	{
		this.Tags.Clear();
	}

	public void Add(Tag tag)
	{
		this.Tags.Add(tag);
	}

	public override bool Filter(IEntryBase entry)
	{
		if(entry.Tags == null)
			return false;

		if(entry.Tags.Matches(this.Tags))
			return true;

		return false;
	}
}
