namespace StudioFourteen.Library.Filters;

public abstract class FilterBase
{
	public abstract bool IsEmpty { get; }

	public abstract void Clear();
	public abstract bool Filter(LibraryEntryBase entry);
}
