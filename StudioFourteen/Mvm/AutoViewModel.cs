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

namespace StudioFourteen.Mvm;

using Newtonsoft.Json;
using StudioFourteen.Mvm;
using Serilog;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public abstract class AutoViewModel : AutoNotify
{
	protected readonly ILogger Log;

	public AutoViewModel()
		: base()
	{
		this.Log = Logging.ForContext(this.GetType());
	}

	[JsonIgnore]
	public ServiceManager Services => ServiceManager.Instance;
}

public abstract class ViewModel : INotifyPropertyChanged
{
	protected readonly ILogger Log;

	public ViewModel()
		: base()
	{
		this.Log = Logging.ForContext(this.GetType());
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	[JsonIgnore]
	public ServiceManager Services => ServiceManager.Instance;

	protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new(propertyName));
	}
}