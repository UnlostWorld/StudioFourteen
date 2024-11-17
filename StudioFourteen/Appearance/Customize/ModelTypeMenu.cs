namespace StudioFourteen.Appearance.Customize;

using Dalamud.Game.ClientState.Objects.Enums;
using Lumina.Excel.Sheets;
using StudioFourteen.GameData;

public class ModelTypeMenu : SelectorMenu
{
	public ModelTypeMenu(Tribe tribe)
		: base(CustomizeIndex.ModelType, ToggleModes.None)
	{
		ModelTypes[] modelTypes = tribe.GetModelTypes();
		foreach (ModelTypes modelType in modelTypes)
		{
			this.Options.Add(new Option(modelType.ToString(), (byte)modelType));
		}
	}

	public override string? Name => "Model Type";
}
