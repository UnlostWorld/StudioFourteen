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

using System;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Interop;
using StudioFourteen.Services;

using Task = System.Threading.Tasks.Task;

public partial class TimeService
	: ServiceBase
{
	private readonly Dictionary<int, string> dayNameLookup = new();
	private readonly Dictionary<int, string> monthNameLookup = new();

	[Notify] private long eorzeaTime;
	[Notify] private int dayOfMonth;
	[Notify] private int minuteOfDay;
	[Notify] private bool freezeTime = false;

	[Notify] private string displayTime = string.Empty;
	[Notify] private string displayMonth = string.Empty;

	private bool isUpdatingEorzeaTime = false;

	public override async Task Start()
	{
		for (int i = 0; i < 12; i++)
		{
			this.monthNameLookup[i] = Resources.Find($"LOC_Time_Month_{i}", i.ToString());
		}

		for (int i = 0; i < 32; i++)
		{
			this.dayNameLookup[i] = Resources.Find($"LOC_Time_Day_{i}", i.ToString());
		}

		await base.Start();
	}

	public override void Attach()
	{
		base.Attach();
		////Hooks.UpdateEorzeaTime.Enable(this.UpdateEorzeaTime);
		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);
	}

	public override void Detach()
	{
		////Hooks.UpdateEorzeaTime.Disable();
		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);
		base.Detach();
	}

	public void Reset()
	{
		this.freezeTime = false;
	}

	protected unsafe void OnGameTick()
	{
		Framework* pFramework = Framework.Instance();
		if (pFramework == null)
			return;

		// Time
		long newEorzeaTime = pFramework->ClientTime.IsEorzeaTimeOverridden ? pFramework->ClientTime.EorzeaTimeOverride : pFramework->ClientTime.EorzeaTime;
		if (this.freezeTime)
		{
			if (pFramework->ClientTime.IsEorzeaTimeOverridden)
			{
				pFramework->ClientTime.EorzeaTimeOverride = (long)this.eorzeaTime;
			}
			else
			{
				pFramework->ClientTime.EorzeaTime = (long)this.eorzeaTime;
			}

			newEorzeaTime = (long)this.eorzeaTime;
		}

		this.EorzeaTime = newEorzeaTime;
	}

	protected void OnEorzeaTimeChanged()
	{
		this.isUpdatingEorzeaTime = true;

		long currentTime = this.EorzeaTime;
		long timeVal = currentTime % 2764800;
		long secondInDay = timeVal % 86400;
		this.MinuteOfDay = (int)(secondInDay / 60f);
		this.DayOfMonth = (int)Math.Floor(timeVal / 86400f);

		TimeSpan displayTime = TimeSpan.FromMinutes(this.MinuteOfDay);

		int hours = displayTime.Hours;
		bool isPm = hours > 12;
		if (isPm)
			hours -= 12;

		int month = DateTime.UtcNow.Month - 1;

		this.DisplayTime = $"{hours}:{displayTime.Minutes.ToString("D2")}{(isPm ? "pm" : "am")}";

		if (this.monthNameLookup.ContainsKey(month))
			this.displayMonth = $"{this.monthNameLookup[month]}, 1577";

		this.isUpdatingEorzeaTime = false;
	}

	protected void OnDayOfMonthChanged(int oldValue, int newValue)
	{
		if (this.isUpdatingEorzeaTime)
			return;

		this.EorzeaTime = (this.MinuteOfDay * 60) + (86400 * (byte)newValue);
	}

	protected void OnMinuteOfDayChanged(int oldValue, int newValue)
	{
		if (this.isUpdatingEorzeaTime)
			return;

		this.EorzeaTime = (newValue * 60) + (86400 * (byte)this.DayOfMonth);
	}

	/*private void UpdateEorzeaTime(IntPtr a1, IntPtr a2)
	{
		if (this.freezeTime)
			return;

		Hooks.UpdateEorzeaTime.Original(a1, a2);
	}*/
}
