namespace StudioFourteen.Library.Filters;

using System;
using System.Collections.Generic;

public class TypeFilter : FilterBase
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

	public override bool Filter(ILibraryEntry entry)
	{
		foreach (Type type in this.types)
		{
			if (entry.IsType(type))
			{
				return true;
			}
		}

		return false;
	}
}
