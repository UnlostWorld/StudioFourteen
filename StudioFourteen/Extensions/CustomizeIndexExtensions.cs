namespace StudioFourteen.Extensions;

using Dalamud.Game.ClientState.Objects.Enums;
using StudioFourteen.Tags;

public static class CustomizeIndexExtensions
{
	public static Tag ToTag(this CustomizeIndex self)
	{
		return Tag.Get(self.ToString());
	}
}
