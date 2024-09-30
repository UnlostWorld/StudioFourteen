namespace StudioFourteen.GameData.Excel;

using StudioFourteen.Tags;

public enum Genders : byte
{
	Masculine,
	Feminine,
}

public static class GendersExtensions
{
	public static TagCollection ToTags(this Genders self)
	{
		TagCollection tags = new();
		tags.Add(self.ToString());
		return tags;
	}
}