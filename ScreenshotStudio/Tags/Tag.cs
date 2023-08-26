// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Tags;

using FontAwesome.Sharp.Pro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using XivToolsWpf;

public class Tag : IEquatable<Tag?>, INotifyPropertyChanged
{
	private string? name;

	public Tag(string name)
	{
		this.Name = name;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public string? Name
	{
		get => this.name;
		set
		{
			this.name = value;
			this.NotifyPropertyChanged();
		}
	}

	public virtual ProIcons Icon => ProIcons.Tag;

	public static implicit operator Tag(string name)
	{
		return new Tag(name);
	}

	public static bool operator ==(Tag? left, Tag? right)
	{
		return EqualityComparer<Tag>.Default.Equals(left, right);
	}

	public static bool operator !=(Tag? left, Tag? right)
	{
		return !(left == right);
	}

	public virtual bool Search(string[]? querry) => SearchUtility.Matches(this.Name, querry);

	public override bool Equals(object? obj)
	{
		return this.Equals(obj as Tag);
	}

	public bool Equals(Tag? other)
	{
		return other is not null && this.Name == other.Name;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(this.Name);
	}

	protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new(propertyName));
	}
}
