namespace StudioFourteen.GameData.Extensions;

using Dalamud.Game.ClientState.Objects.Enums;
using Lumina.Excel.Sheets;

public static class CharaMakeTypeExtensions
{
	public static CharaMakeType.CharaMakeStructStruct? GetMenu(this CharaMakeType self, CustomizeIndex index)
	{
		foreach (CharaMakeType.CharaMakeStructStruct menu in self.CharaMakeStruct)
		{
			if (menu.Customize == (uint)index)
			{
				return menu;
			}
		}

		return null;
	}
}
