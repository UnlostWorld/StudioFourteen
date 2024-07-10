namespace ScreenshotStudio.Library.Filters;

using System;
using System.Collections.Generic;

internal class TypeFilter : FilterBase
{
    private readonly HashSet<Type> types = new();

    public TypeFilter(string name, params Type[] loadTypes)
        : base(name)
    {
        foreach(Type type in loadTypes)
        {
            this.types.Add(type);
        }
    }

    public IEnumerable<Type> Types => this.types;

    public override void Clear()
    {
        this.types.Clear();
    }

    public override bool Filter(EntryBase entry)
    {
        if(entry is ItemEntryBase file)
        {
            return this.types.Contains(file.LoadsType);
        }

        return true;
    }
}
