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

namespace StudioFourteen.Files;

using System;
using System.Threading.Tasks;
using StudioFourteen.Environment;
using StudioFourteen.Services;

public class EnvironmentFileTypeInfo : JsonFileTypeInfoBase<EnvironmentFile>
{
	public override string Extension => ".env";
	public override string TypeName => "Environment";
	public override object? Icon => Resources.Find("ICON_Library_Entry_Environment");
}

[Serializable]
public class EnvironmentFile : FileBase
{
	public EnvironmentState? State { get; set; }
	public byte? WeatherId { get; set; }
	public long? Time { get; set; }
	public uint? TerritoryId { get; set; }

	public Task Save()
	{
		this.State = ServiceManager.Instance.Environment.CurrentState;

		if (ServiceManager.Instance.Environment.CurrentWeather != null)
			this.WeatherId = (byte)ServiceManager.Instance.Environment.CurrentWeather.RowId;

		if (ServiceManager.Instance.Time.FreezeTime)
			this.Time = ServiceManager.Instance.Time.EorzeaTime;

		this.TerritoryId = ServiceManager.Instance.Territory.CurrentTerritory?.RowId;

		return Task.CompletedTask;
	}

	public async Task Apply()
	{
		await TickService.GameTick();

		if (this.State != null)
			ServiceManager.Instance.Environment.CurrentState = this.State;

		if (this.WeatherId != null)
			ServiceManager.Instance.Environment.ChangeWeather((byte)this.WeatherId);

		if (this.Time != null)
		{
			ServiceManager.Instance.Time.FreezeTime = true;
			ServiceManager.Instance.Time.EorzeaTime = (long)this.Time;
		}

		if (this.TerritoryId != null)
		{
			ServiceManager.Instance.Territory.ChangeTerritory((uint)this.TerritoryId);
		}
	}
}