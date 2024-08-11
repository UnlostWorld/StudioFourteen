namespace ScreenshotStudio;

using System.Threading.Tasks;

public interface ICharacterApplicable
{
	public Task Apply(int objectTableIndex);
}

public interface ICharacterRevertible
{
	public Task Revert(int objectTableIndex);
}