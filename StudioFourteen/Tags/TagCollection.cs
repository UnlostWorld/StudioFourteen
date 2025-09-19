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

using System.Text;
using StudioFourteen.Extensions;

public class TagCollection : FastObservableCollection<Tag>
{
	public static readonly TagCollection Empty = new();

	public TagCollection()
	{
	}

	public TagCollection(params string[] tags)
	{
		foreach (string tag in tags)
		{
			this.Add(tag);
		}
	}

	public TagCollection(TagCollection other)
		: this()
	{
		this.AddRange(other);
	}

	public bool IsReadOnly => true;

	public void Add(TagCollection? tags)
	{
		if (tags == null)
			return;

		this.AddRange(tags);
	}

	public void AddSafe(string? name)
	{
		if (name == null)
			return;

		this.Add(name);
	}

	public Tag Add(string name)
	{
		Tag tag = Tag.Get(name);
		this.Add(tag);
		return tag;
	}

	public bool Matches(TagCollection other)
	{
		foreach (Tag tag in other)
		{
			if (!this.Contains(tag))
			{
				return false;
			}
		}

		return true;
	}

	public override string ToString()
	{
		StringBuilder builder = new();
		foreach (Tag tag in this)
		{
			builder.Append(tag.Name);
			builder.Append(' ');
		}

		return builder.ToString();
	}

	protected override void InsertItem(int index, Tag item)
	{
		if (this.Contains(item))
			return;

		base.InsertItem(index, item);
	}

	protected override void SetItem(int index, Tag item)
	{
		if (this.Contains(item))
			return;

		base.SetItem(index, item);
	}
}