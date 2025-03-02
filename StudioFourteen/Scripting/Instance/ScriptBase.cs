// .                    @@             _____ _______ _    _ _____ _____ ____
//          @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//         @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//         @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//        @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//    @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//     @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//      @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//      @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//    @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//     @@@@             @@@        https://github.com/UnlostWorld/StudioFourteen
//       @@@@@      @@@@@
//        @@@@@@@@@@@@@@                This software is licensed under the
//            @@@@  @                  GNU AFFERO GENERAL PUBLIC LICENSE v3

namespace StudioFourteen.Scripting.Instance;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// The base class of all script instances.
/// Be cautious what is exposed here as we don't want scripts
/// getting out of the Scripting.Instance namespace for security reasons.
/// </summary>
public class ScriptBase
{
	private readonly ScriptLogger log = new();
	private readonly LibraryInterface library = new();
	private readonly PhotoInterface photo = new();
	private readonly StatusInterface status = new();
	private readonly CharacterInterface character = new();
	private readonly OptionsInterface options = new();

	public ScriptLogger Log
	{
		get
		{
			this.CancellationToken.ThrowIfCancellationRequested();
			return this.log;
		}
	}

	public LibraryInterface Library
	{
		get
		{
			this.CancellationToken.ThrowIfCancellationRequested();
			return this.library;
		}
	}

	public PhotoInterface Photo
	{
		get
		{
			this.CancellationToken.ThrowIfCancellationRequested();
			return this.photo;
		}
	}

	public StatusInterface Status
	{
		get
		{
			this.CancellationToken.ThrowIfCancellationRequested();
			return this.status;
		}
	}

	public CharacterInterface Character
	{
		get
		{
			this.CancellationToken.ThrowIfCancellationRequested();
			return this.character;
		}
	}

	public OptionsInterface Options
	{
		get
		{
			this.CancellationToken.ThrowIfCancellationRequested();
			return this.options;
		}
	}

	public CancellationToken CancellationToken { get; set; }

	public async Task Delay(int millisecondsDelay)
	{
		await Task.Delay(millisecondsDelay, this.CancellationToken);
		this.CancellationToken.ThrowIfCancellationRequested();
	}
}