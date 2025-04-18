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
using StudioFourteen.Interop.Structs.Environment;
using StudioFourteen.Interop;
using StudioFourteen.Services;
using System.Threading.Tasks;
using StudioFourteen.GameData.Library;

public partial class EnvironmentService
	: ServiceBase
{
	public readonly SkyTextureSource SkyTextureSource = new();

	[Notify] private WeatherLibraryEntry? currentWeather;
	[Notify] private EnvironmentState currentState = new();

	private bool isReadingWeather;

	public override Task Initialize()
	{
		this.Services.Library.AddSource(this.SkyTextureSource);
		return base.Initialize();
	}

	public unsafe override void Attach()
	{
		base.Attach();

		Hooks.EnvStateCopy.Enable(this.EnvStateCopy);
		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);
	}

	public override void Detach()
	{
		Hooks.EnvStateCopy.Disable();
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
		this.ChangeWeather((byte)weather.RowId);
	}

	public unsafe void ChangeWeather(byte weatherId)
	{
		TickService.VerifyGameTickThread();

		EnvManagerEx* pEnvironmentManager = EnvManagerEx.Instance();
		if (pEnvironmentManager == null)
			return;

		pEnvironmentManager->ActiveWeather = weatherId;
		pEnvironmentManager->TransitionTime = 0;
	}

	public async Task Export()
	{
		EnvironmentFile file = new();
		await file.Save();
		this.Services.Files.SaveFile(file, $"Environment");
	}

	public void Reset()
	{
		this.CurrentState = new();
	}

	protected unsafe void OnGameTick()
	{
		EnvManagerEx* pEnvironmentManager = EnvManagerEx.Instance();
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

	private unsafe nint EnvStateCopy(EnvState* dest, EnvState* src)
	{
		nint result = Hooks.EnvStateCopy.Original(dest, src);
		this.CurrentState.ReadFrom(dest);
		this.CurrentState.WriteTo(dest);

		return result;
	}
}
