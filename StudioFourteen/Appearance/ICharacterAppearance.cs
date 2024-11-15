namespace StudioFourteen.Appearance;

using System.Threading.Tasks;

public interface ICharacterAppearance
{
	string? Name { get; }

	public Task Apply(int objectTableIndex);
	public Task Spawn();
}