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

namespace StudioFourteen;

using StudioFourteen.Plugin;
using StudioFourteen.Studio;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

public static class Logging
{
	public static readonly LoggerConfiguration Configuration;
	private static readonly ILogger Logger;

	static Logging()
	{
		Formatter formatter = new();

		Configuration = new LoggerConfiguration();
		Configuration.Enrich.With<StackEnricher>();
		Configuration.WriteTo.Sink(new DebugSink(formatter));
		////Configuration.WriteTo.Sink(new ErrorWindowSink());
		Configuration.WriteTo.Sink(new ErrorReportingSink());
		Configuration.WriteTo.Sink(new DalamudSink(formatter));

		Logger = Configuration.CreateLogger();

		WpfUtils.Logging.Log.HandleMessage = WpfLog;
		WpfUtils.Logging.Log.HandleError = WpfError;
	}

	public static ILogger Shared => Logger;

	public static ILogger ForContext<T>() => ForContext(typeof(T));
	public static ILogger ForContext(Type type) => ForContext(type.Name);

	public static ILogger ForContext(string context)
	{
		return Logger.ForContext("Context", context);
	}

	public static void WpfLog(string message) => Shared.Information(message);
	public static void WpfError(Exception? ex, string message) => Shared.Error(ex, message);

	public static void Information(string message) => Shared.Information(message);
}

public class Formatter : ITextFormatter
{
	public void Format(LogEvent logEvent, TextWriter output)
	{
		output.Write("[");
		output.Write(logEvent.Level);
		output.Write("] ");

		if (logEvent.Properties.TryGetValue("Context", out var contextValue))
		{
			output.Write("[");
			if (contextValue is ScalarValue sv)
			{
				output.Write(sv.Value);
			}

			output.Write("] ");
		}

		output.WriteLine(logEvent.MessageTemplate);

		if (logEvent.Properties.TryGetValue("StackTrace", out var stackTrace))
		{
			if (stackTrace is ScalarValue sv)
			{
				output.WriteLine(CleanStackTrace(sv.Value as string));
			}
		}

		Exception? ex = logEvent.Exception;
		while (ex != null)
		{
			output.Write("----> ");
			output.WriteLine(ex.Message);
			output.WriteLine(CleanStackTrace(ex.StackTrace));
			ex = ex.InnerException;
		}
	}

	private static string? CleanStackTrace(string? stack)
	{
		if (stack == null)
			return null;

		string[] lines = stack.Split('\n');

		StringBuilder stackBuilder = new();
		for (int i = 0; i < lines.Length; i++)
		{
			string line = lines[i];
			line = line.TrimEnd('\r', '\n');

			// replace 'SomeFile.cs:line 116' with 'SomeFile.cs:116" so the line is clickable in VsCode.
			line = line.Replace(":line ", ":");

			// Shorten the file paths unless we are actively debugging (so they stay clickable)
			if (!Debugger.IsAttached)
			{
				line = line.Replace("C:\\Projects\\StudioFourteen\\", "\\");
			}

			stackBuilder.AppendLine(line);
		}

		return stackBuilder.ToString();
	}
}

public class StackEnricher : ILogEventEnricher
{
	public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
	{
		if (logEvent.Level >= LogEventLevel.Error && logEvent.Exception == null)
		{
			logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("StackTrace", new StackTrace(7, true)));
		}
	}
}

/*public class ErrorWindowSink : ILogEventSink
{
	public void Emit(LogEvent logEvent)
	{
		if (logEvent.Level >= LogEventLevel.Error)
		{
			ErrorWindow.Show($"{logEvent.MessageTemplate.Text}\n{logEvent.Exception?.Message}");
		}
	}
}*/

public class ErrorReportingSink : ILogEventSink
{
	public void Emit(LogEvent logEvent)
	{
		ServiceManager.Instance.Errors.HandleLog(logEvent);
	}
}

public class DalamudSink : ILogEventSink
{
	private readonly ITextFormatter formatter;

	public DalamudSink(ITextFormatter formatter)
	{
		this.formatter = formatter;
	}

	public void Emit(LogEvent logEvent)
	{
		StringWriter writer = new();
		this.formatter.Format(logEvent, writer);
		string message = writer.ToString();
		message = message.TrimEnd('\r', '\n');

		// Unsure why PluginLog.LogRaw doesn't work. possibly due to the Serilog.LogEventLevel not matching up?
		switch (logEvent.Level)
		{
			case LogEventLevel.Verbose:
			{
				DalamudServices.Log?.Verbose(message);
				break;
			}

			case LogEventLevel.Debug:
			{
				DalamudServices.Log?.Debug(message);
				break;
			}

			case LogEventLevel.Information:
			{
				DalamudServices.Log?.Information(message);
				break;
			}

			case LogEventLevel.Warning:
			{
				DalamudServices.Log?.Warning(message);
				break;
			}

			case LogEventLevel.Error:
			case LogEventLevel.Fatal:
			{
				DalamudServices.Log?.Error(message);
				break;
			}
		}
	}
}

public class DebugSink : ILogEventSink
{
	private readonly ITextFormatter formatter;

	public DebugSink(ITextFormatter formatter)
	{
		this.formatter = formatter;
	}

	public void Emit(LogEvent logEvent)
	{
		StringWriter writer = new();
		this.formatter.Format(logEvent, writer);
		string message = writer.ToString();
		message = GetColorCode(logEvent.Level) + message.TrimEnd('\r', '\n');
		System.Diagnostics.Debug.WriteLine(message);
	}

	private static string GetColorCode(LogEventLevel level)
	{
		switch (level)
		{
			case LogEventLevel.Warning: return "\u001b[1;33m ";
			case LogEventLevel.Error: return "\u001b[1;31m ";
			case LogEventLevel.Fatal: return "\u001b[1;31m ";
		}

		return string.Empty;
	}
}