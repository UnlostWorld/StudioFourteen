namespace StudioFourteen.Online;

using StudioFourteen.Services;
using System.IO;
using System.Threading.Tasks;

public class OnlineService : ServiceBase
{
	public DirectoryInfo? FileCache { get; private set; }

	public override async Task Initialize()
	{
		await base.Initialize();

		this.FileCache = new($"{this.Services.Files.StudioFourteenAppDataDir}/Online/");

		if (!Directory.Exists(this.FileCache.FullName))
		{
			Directory.CreateDirectory(this.FileCache.FullName);
		}
	}
}