namespace ScreenshotStudio.Library.Filters;

using System;
using System.Collections.Generic;

internal class TypeFilter : FilterBase
{
	private readonly HashSet<Type> types = new();

	public TypeFilter(params Type[] loadTypes)
	{
		foreach(Type type in loadTypes)
		{
			this.types.Add(type);
		}
	}

	public IEnumerable<Type> Types => this.types;

	public override bool IsEmpty => this.types.Count == 0;

	public override void Clear()
	{
		this.types.Clear();
	}

	public override bool Filter(IEntryBase entry)
	{
		if(entry is ItemEntryBase file)
		{
			return this.types.Contains(file.LoadsType);
		}

		foreach (Type type in this.types)
		{
			if (entry.GetType().IsAssignableTo(type))
			{
				return true;
			}
		}

		return false;
	}
}
