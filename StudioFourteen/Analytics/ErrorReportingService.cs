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

namespace StudioFourteen.Analytics;

using System;
using System.IO;
using System.Threading.Tasks;
using Serilog.Events;
using StudioFourteen.Services;
using StudioOnline.Analytics;
using StudioFourteen.Utils;

public class ErrorReportingService : ServiceBase
{
	private readonly FuncQueue reportQueue;
	private string? lastMessage;
	private int errorCount = 0;

	public ErrorReportingService()
	{
		this.reportQueue = new(this.SendReport, 1000);
	}

	public void HandleLog(LogEvent logEvent)
	{
		if (logEvent.Level >= LogEventLevel.Error)
		{
			this.lastMessage = logEvent.RenderMessage();
			this.errorCount++;
			this.reportQueue.Invoke();
		}

		if (logEvent.Level == LogEventLevel.Fatal)
		{
			this.reportQueue.InvokeImmediate();
		}
	}

	private async Task SendReport()
	{
		ErrorReportPanel? panel = await this.Services.Panels.GamePanels.SetIsOpenAsync<ErrorReportPanel>(true, true);
		if (panel != null)
		{
			panel.ShortCode = null;
			panel.ErrorMessage = this.lastMessage;
			panel.IsSending = true;
			panel.ReportingEnabled = this.Settings.SendErrorReports;
		}

		ErrorReport report = new ErrorReport();
		report.Message = $"x{this.errorCount} - {this.lastMessage}";

		// TODO: We should get this log path from dalamud somehow.
		string logPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/XIVLauncher/dalamud.log";
		if (File.Exists(logPath))
		{
			using var fileStream = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			using var textReader = new StreamReader(fileStream);
			report.LogFile = textReader.ReadToEnd();
		}

		string shortCode;
		#if DEBUG
		{
			shortCode = "DEBUG";
		}
		#else
		{
			shortCode = await report.Send();
		}
		#endif

		this.Log.Information($"Sent log. Got shortcode: {shortCode}");

		if (shortCode.Length > 10)
			shortCode = "INVALID";

		if (panel != null)
			{
				panel.IsSending = false;
				panel.ShortCode = shortCode;
			}
	}
}