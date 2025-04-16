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

namespace StudioFourteen.Environment;

using PropertyChanged.SourceGenerator;
using Lumina.Excel.Sheets;
using StudioFourteen.Services;
using System.Threading.Tasks;
using StudioFourteen.GameData.Library;
using FFXIVClientStructs.FFXIV.Client.Graphics.Environment;

public partial class EnvironmentService
	: ServiceBase
{
	[Notify] private WeatherLibraryEntry? currentWeather;

	private bool isReadingWeather;

	public unsafe override void Attach()
	{
		base.Attach();
		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);
	}

	public override void Detach()
	{
		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);
		base.Detach();
	}

	public async Task ChangeWeatherAsync(Weather weather)
	{
		await TickService.GameTick();
		this.ChangeWeather(weather);
	}

	public unsafe void ChangeWeather(Weather weather)
	{
		TickService.VerifyGameTickThread();

		EnvManager* pEnvironmentManager = EnvManager.Instance();
		if (pEnvironmentManager == null)
			return;

		pEnvironmentManager->ActiveWeather = (byte)weather.RowId;
		pEnvironmentManager->TransitionTime = 0;
	}

	protected unsafe void OnGameTick()
	{
		EnvManager* pEnvironmentManager = EnvManager.Instance();
		if (pEnvironmentManager == null)
			return;

		// Weather
		byte weatherId = pEnvironmentManager->ActiveWeather;
		if (weatherId != this.currentWeather?.RowId)
		{
			this.isReadingWeather = true;
			this.CurrentWeather = this.Services.GameData.GetLibraryEntry<WeatherLibraryEntry>(weatherId);
			this.isReadingWeather = false;
		}
	}

	private void OnCurrentWeatherChanged(WeatherLibraryEntry? oldValue, WeatherLibraryEntry? newValue)
	{
		if (this.isReadingWeather || newValue == null)
			return;

		this.Services.Tick.Dispatch(TickService.Channels.GameTick, () => this.ChangeWeather(newValue.Excel));
	}
}
