namespace StudioFourteen.GameData.Excel;

using StudioFourteen.Library;
using StudioFourteen.Library.Sources;
using StudioFourteen.Tags;
using System;
using WpfUtils;

public abstract class LibraryExcelRow : StudioExcelRow, ILibraryEntry
{
	public TagCollection Tags { get; init; } = new();
	public string? Name { get; set; }
	public bool IsVisible { get; set; }
	public SourceBase? Source { get; set; }
	public string? SourceInfo { get; set; }
	public string Identifier => $"{this.GetType().Name} #{this.RowId}";

	public bool IsFavorite
	{
		get => LibraryFavoritesFilter.GetIsFavorite(this);
		set
		{
			LibraryFavoritesFilter.SetIsFavorite(this, value);
			this.NotifyPropertyChanged();
		}
	}

	public bool IsType(Type type) => this.GetType().IsAssignableTo(type);

	public void Dispose()
	{
	}

	public virtual bool Search(string[]? query)
	{
		bool result = false;
		result |= SearchUtility.Matches(this.RowId, query);
		result |= SearchUtility.Matches(this.Name, query);
		return result;
	}
}