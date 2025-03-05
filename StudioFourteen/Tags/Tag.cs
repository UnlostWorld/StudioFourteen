// .                    @@             _____ _______ _    _ _____ _____ ____
//          @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//         @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//         @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//        @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//    @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//     @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//      @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//      @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//    @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//     @@@@             @@@        https://github.com/UnlostWorld/StudioFourteen
//       @@@@@      @@@@@
//        @@@@@@@@@@@@@@                This software is licensed under the
//            @@@@  @                  GNU AFFERO GENERAL PUBLIC LICENSE v3

namespace StudioFourteen.Tags;

using System;
using System.Collections.Generic;
using System.Windows.Media;
using WpfUtils;

public class Tag : IEquatable<Tag?>
{
	private static readonly Dictionary<string, Tag> TagCache = new();

	private readonly string name;
	private readonly HashSet<string> aliases = new();

	private string? displayName;
	private string? toolTip;

	private Tag(string name)
	{
		this.name = name;
	}

	public string Name => this.name;
	public IReadOnlyCollection<string> Aliases => this.aliases;

	public string DisplayName
	{
		get
		{
			if (this.displayName == null)
				this.displayName = StudioFourteen.Resources.Find($"LOC_Tag_{this.name}", this.name);

			return this.displayName;
		}
	}

	public string ToolTip
	{
		get
		{
			if (this.toolTip == null)
				this.toolTip = StudioFourteen.Resources.Find($"LOC_Tag_{this.name}_ToolTip", this.name);

			return this.toolTip;
		}
	}

	public static implicit operator Tag(string name)
	{
		return Tag.Get(name);
	}

	public static bool operator ==(Tag? left, Tag? right)
	{
		return EqualityComparer<Tag>.Default.Equals(left, right);
	}

	public static bool operator !=(Tag? left, Tag? right)
	{
		return !(left == right);
	}

	public static Tag Get(string name)
	{
		if (string.IsNullOrEmpty(name))
			throw new InvalidOperationException("Attempt to get empty tag");

		lock (TagCache)
		{
			Tag? tag = null;
			if (TagCache.TryGetValue(name, out tag) && tag != null)
				return tag;

			tag = new(name);
			TagCache.Add(name, tag);
			return tag;
		}
	}

	public static void ClearTagCache()
	{
		TagCache.Clear();
	}

	public static int TagCount()
	{
		return TagCache.Count;
	}

	public Tag WithAlias(string? alias)
	{
		if (alias == null)
			return this;

		lock (this.aliases)
		{
			this.aliases.Add(alias);
		}

		return this;
	}

	public virtual bool Search(string[]? querry)
	{
		if (SearchUtility.Matches(this.Name, querry))
			return true;

		if (SearchUtility.Matches(this.DisplayName, querry))
			return true;

		foreach (string alias in this.aliases)
		{
			if (SearchUtility.Matches(alias, querry))
			{
				return true;
			}
		}

		return false;
	}

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
}
