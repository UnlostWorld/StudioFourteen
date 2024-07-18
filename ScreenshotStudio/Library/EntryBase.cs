namespace ScreenshotStudio.Library;

using ScreenshotStudio.Library.Sources;
using ScreenshotStudio.Tags;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WpfUtils;

public interface IEntryBase : IDisposable
{
	string? Name { get; }
	TagCollection Tags { get; }
	SourceBase? Source { get; }
	string Identifier { get; }
	bool IsValid { get; }

	double Search(string[] query);
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
	public virtual bool IsVisible { get; set; }
	public TagCollection Tags { get; init; } = new();
	public SourceBase? Source => this.source;

	public virtual bool IsValid => true;

	public string Identifier => $"{this.Source?.GetInternalId()}||{this.GetInternalId()}";

	public virtual double Search(string[] query)
	{
		return SearchUtility.Search(this.Name, query);
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
