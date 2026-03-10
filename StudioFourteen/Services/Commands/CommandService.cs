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

namespace StudioFourteen.Services.Content;

using System;
using System.Collections.Generic;
using global::Dalamud.Game.Command;

public class CommandService : IService
{
	private readonly Dictionary<string, Action> callbacks = new();

	public void Dispose()
	{
		foreach ((string command, Action callback) in this.callbacks)
		{
			Studio.DalamudCommandManager.RemoveHandler(command);
		}
	}

	public void AddCommand(string command, string description, Action callback)
	{
		if (!command.StartsWith('/'))
			command = '/' + command;

		if (this.callbacks.ContainsKey(command))
			throw new Exception($"Command: {command} already registered");

		this.callbacks[command] = callback;
		Studio.DalamudCommandManager.AddHandler(command, new CommandInfo(this.HandleCommand)
		{
			HelpMessage = description,
		});
	}

	private void HandleCommand(string command, string arguments)
	{
		if (this.callbacks.ContainsKey(command))
		{
			try
			{
				this.callbacks[command].Invoke();
			}
			catch (Exception ex)
			{
				Studio.Log.Error(ex, "Error handling command");
			}
		}
	}
}