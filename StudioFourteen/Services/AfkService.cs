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

using System.Diagnostics;
using Dalamud.Game.ClientState.Keys;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.System.Input;
using StudioFourteen.Services;

////[Service]
public class AfkService : ServiceBase
{
	private readonly Stopwatch stopwatch = new();

	public override void Attach()
	{
		this.stopwatch.Start();
		this.Services.Tick.Add(TickService.Channels.StudioTick, this.OnTick);
		base.Attach();
	}

	public override void Detach()
	{
		this.stopwatch.Stop();
		this.Services.Tick.Remove(TickService.Channels.StudioTick, this.OnTick);
		base.Detach();
	}

	private void OnTick()
	{
		if (this.stopwatch.ElapsedMilliseconds < 1000)
			return;

		this.stopwatch.Restart();

		unsafe
		{
			Framework.Instance()->KeyboardInputs.KeyState[(int)VirtualKey.CONTROL] = KeyStateFlags.Down;
		}
	}
}