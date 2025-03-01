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

using Serilog.Events;
using System;

public class ScriptLogger(ScriptPanel outputPanel)
{
	public void Information(string message) => outputPanel.AppendLog(LogEventLevel.Information, message);
	public void Warning(string message) => outputPanel.AppendLog(LogEventLevel.Warning, message);
	public void Error(string message) => outputPanel.AppendLog(LogEventLevel.Error, message);
	public void Error(Exception ex, string message) => outputPanel.AppendLog(LogEventLevel.Error, message);

	public void Information(string message, string? location) => outputPanel.AppendLog(LogEventLevel.Information, message, location);
	public void Warning(string message, string? location) => outputPanel.AppendLog(LogEventLevel.Warning, message, location);
	public void Error(string message, string? location) => outputPanel.AppendLog(LogEventLevel.Error, message, location);
	public void Error(Exception ex, string message, string? location) => outputPanel.AppendLog(LogEventLevel.Error, message, location);
}