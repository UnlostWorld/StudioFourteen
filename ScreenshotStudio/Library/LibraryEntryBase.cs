namespace ScreenshotStudio.Library;

using ScreenshotStudio.Library.Executors;
using ScreenshotStudio.Library.Sources;
using ScreenshotStudio.Tags;
using Serilog;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WpfUtils;

public delegate void EntryEvent();

public interface ILibraryEntry : IDisposable
{
	string? Name { get; }
	TagCollection Tags { get; }
	SourceBase? Source { get; }
	string Identifier { get; }
	bool IsValid { get; }

	bool Search(string[] query);

	EntryExecutor? GetExecutor();
}

/// <summary>
/// An entry is a library object.
/// </summary>
public abstract class LibraryEntryBase : ITagged, ILibraryEntry, INotifyPropertyChanged
{
	protected readonly ILogger Log;

	private readonly SourceBase? source;

	public LibraryEntryBase(SourceBase? source)
	{
		this.source = source;
		this.Log = Logging.ForContext(this.GetType());
	}

	public event PropertyChangedEventHandler? PropertyChanged;
	public event EntryEvent? ExecuteRequested;

	public abstract string Name { get; }
	public virtual bool IsVisible { get; set; }
	public TagCollection Tags { get; init; } = new();
	public SourceBase? Source => this.source;

	public virtual bool IsValid => true;

	public string Identifier => $"{this.Source?.GetInternalId()}||{this.GetInternalId()}";

	public abstract EntryExecutor? GetExecutor();

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

public abstract class LibraryEntryBase<T> : LibraryEntryBase
	where T : EntryExecutor
{
	protected LibraryEntryBase(SourceBase? source)
		: base(source)
	{
	}

	public sealed override EntryExecutor? GetExecutor()
	{
		return Activator.CreateInstance(typeof(T), [this]) as EntryExecutor;
	}
}
