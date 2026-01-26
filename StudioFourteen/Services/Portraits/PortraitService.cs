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

namespace StudioFourteen.Services.Portraits;

using System;
using System.Collections.Generic;
using StudioFourteen.Services.Tick;

public class PortraitService : IService
{
	private readonly Queue<Portrait> pending = new();
	private Portrait? current;

	public PortraitService()
	{
		Studio.Tick.Add(TickChannels.Game, this.OnGameTick);
	}

	public void Dispose()
	{
		Studio.Tick.Remove(TickChannels.Game, this.OnGameTick);
	}

	public void Generate(int objectIndex, Action<string> callback)
	{
		Portrait generator = new(objectIndex, callback);
		this.pending.Enqueue(generator);
	}

	private void OnGameTick()
	{
		if (this.current != null)
		{
			this.current.OnGameTick();

			if (this.current.IsDone)
				this.current = null;

			return;
		}

		if (this.pending.TryDequeue(out Portrait? generator) && generator != null)
		{
			this.current = generator;
		}
	}
}