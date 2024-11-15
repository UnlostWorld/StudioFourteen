namespace StudioFourteen.GameData.Extensions;

using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using StudioFourteen.Tags;
using System.Text;

public static class ClassJobExtensions
{
	public enum Roles : byte
	{
		None,
		Tank,
		MeleeDamage,
		RangedDamage,
		Healer,
	}

	public static Roles GetRole(this ClassJob self)
	{
		return (Roles)self.Role;
	}

	public static bool GetIsClass(this ClassJob self) => self.ClassJobParent.RowId == self.RowId;
	public static bool GetIsJob(this ClassJob self) => !self.GetIsClass();

	public static TagCollection ToTags(this ClassJob self)
	{
		TagCollection tags = new();

		string? name = self.NameEnglish.GetString();
		string? abbreviation = self.Abbreviation.GetString();
		if (name != null)
			tags.Add(name).WithAlias(abbreviation);

		if (self.GetRole() != Roles.None)
			tags.Add(self.Role.ToString());

		if (self.GetIsClass())
		{
			tags.Add("Class");
		}
		else
		{
			tags.Add("Job");
		}

		if (self.IsLimitedJob)
		{
			tags.Add("Limited");
		}

		return tags;
	}

	public static Tag? ToExclusiveTag(this ClassJob self)
	{
		string? name = self.NameEnglish.GetString();
		string? abbreviation = self.Abbreviation.GetString();

		if (name == null)
			return null;

		return Tag.Get($"{name} exclusive").WithAlias($"{abbreviation} exclusive");
	}
}
