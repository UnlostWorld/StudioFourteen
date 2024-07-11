namespace ScreenshotStudio.Library;

using ScreenshotStudio.Library.Filters;
using ScreenshotStudio.Library.Sources;
using ScreenshotStudio.Tags;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WpfUtils;

public interface IEntryBase : IDisposable
{
	string? Name { get; }
	//// object Icon {get;}
	bool IsVisible { get; set; }
	TagCollection Tags { get; }
	SourceBase? Source { get; }
	string? SourceInfo { get; set; }
	string Identifier { get; }

	bool PassesFilters(params FilterBase[] filters);
	bool Search(string[] query);
}

/// <summary>
/// An entry is a library object.
/// </summary>
public abstract class EntryBase : ITagged, IEntryBase, INotifyPropertyChanged
{
	private readonly SourceBase? source;

	public EntryBase(SourceBase? source)
	{
		this.source = source;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public abstract string Name { get; }
	//// public object Icon {get;}
	public virtual bool IsVisible { get; set; }
	public TagCollection Tags { get; init; } = new();
	public SourceBase? Source => this.source;
	public string? SourceInfo { get; set; }

	public string Identifier => $"{this.Source?.GetInternalId()}||{this.GetInternalId()}";

	public abstract bool PassesFilters(params FilterBase[] filters);

	public virtual bool Search(string[] query)
	{
		return SearchUtility.Matches(this.Name, query);
	}

	public virtual void Dispose()
	{
	}

	public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected abstract string GetInternalId();
}
