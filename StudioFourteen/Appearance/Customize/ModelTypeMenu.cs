namespace StudioFourteen.Appearance.Customize;

using Dalamud.Game.ClientState.Objects.Enums;
using Lumina.Excel.Sheets;
using StudioFourteen.GameData;

public class ModelTypeMenu : SelectorMenu
{
	private readonly string name;

	public ModelTypeMenu(Tribe tribe, string name)
		: base(CustomizeIndex.ModelType, ToggleModes.None)
	{
		this.name = name;

		ModelTypes[] modelTypes = tribe.GetModelTypes();
		foreach (ModelTypes modelType in modelTypes)
		{
			this.Options.Add(new Option(modelType.ToString(), (byte)modelType));
		}
	}

	public override string? Name => this.name;
}
