namespace StudioFourteen.Appearance;

using System.Threading.Tasks;
using System.Windows.Input;

public interface ICharacterAppearance
{
	string? Name { get; }

	ICommand ApplyCommand { get; }
	ICommand RevertCommand { get; }
	ICommand SpawnCommand { get; }

	public Task Apply(int objectTableIndex);
	public Task Spawn(int objectTableIndex);
}