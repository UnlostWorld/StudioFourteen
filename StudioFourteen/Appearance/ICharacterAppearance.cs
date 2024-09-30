namespace ScreenshotStudio.Appearance;

using System.Threading.Tasks;
using System.Windows.Input;

public interface ICharacterAppearance
{
	string? Name { get; }

	public Task Apply(int objectTableIndex);
}