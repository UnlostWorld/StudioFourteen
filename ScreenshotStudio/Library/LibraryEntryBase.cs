namespace ScreenshotStudio.Library;

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

	bool IsType(Type type);
	bool Search(string[] query);
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

	public ServiceManager Services => ServiceManager.Instance;

	public abstract string Name { get; }
	public virtual bool IsVisible { get; set; }
	public TagCollection Tags { get; init; } = new();
	public SourceBase? Source => this.source;

	public virtual bool IsValid => true;

	public string Identifier => $"{this.Source?.GetInternalId()}||{this.GetInternalId()}";

	public virtual bool IsType(Type type) => this.GetType().IsAssignableTo(type);
	public virtual bool Search(string[] query) => SearchUtility.Matches(this.Name, query);

	public virtual void Dispose()
	{
	}

	public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected abstract string GetInternalId();
}