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

namespace StudioFourteen.Scripting;

using PropertyChanged.SourceGenerator;
using Serilog.Events;
using StudioFourteen.Panels;
using System.Threading.Tasks;
using System.Windows;
using WpfUtils.Extensions;

public partial class ScriptPanel : Panel
{
	[Notify] private string status = string.Empty;
	[Notify] private double progress = 0;
	[Notify] private bool isIndeterminate = true;

	public FastObservableCollection<LogEntry> ScriptLog { get; init; } = new();

	public static async Task<ScriptPanel?> Show()
	{
		return await ServiceManager.Instance.Panels.Open<ScriptPanel>();
	}

	public void SetTitle(string title)
	{
		this.Dispatcher.Invoke(() =>
		{
			this.Subtitle = title;
		});
	}

	public void SetStatus(string status)
	{
		this.Status = status;

		this.AppendLog(LogEventLevel.Information, status);
	}

	public void SetProgress(double? progress)
	{
		if (progress == null)
		{
			this.IsIndeterminate = true;
		}
		else
		{
			this.IsIndeterminate = false;
			this.Progress = (double)progress * 100;
		}
	}

	public void AppendLog(LogEventLevel level, string message, string? location = null)
	{
		this.Dispatcher.Invoke(() =>
		{
			this.ScriptLog.Add(new(level, message, location));
			this.Scroller.ScrollToBottom();
		});
	}

	private void OnCloseClicked(object sender, RoutedEventArgs e)
	{
		this.Close();
	}
}

public class LogEntry(LogEventLevel level, string message, string? location)
{
	public LogEventLevel Level { get; init; } = level;
	public string Message { get; init; } = message;
	public string? Location { get; init; } = location;
}