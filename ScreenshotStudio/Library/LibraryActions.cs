namespace ScreenshotStudio.Library;

using System.Threading.Tasks;

public interface ILibraryActions
{
	public Task Apply(int objectTableIndex);
}