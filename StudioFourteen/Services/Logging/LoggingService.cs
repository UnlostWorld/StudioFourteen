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

namespace StudioFourteen.Services.Logging;

using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

public class LoggingService : IDisposable
{
	public readonly LoggerConfiguration Configuration;
	private readonly ILogger logger;

	public LoggingService()
	{
		this.Configuration = new LoggerConfiguration();
		this.Configuration.Enrich.With<StackEnricher>();
		this.Configuration.WriteTo.Sink(new DebugSink(new Formatter(true)));
		////this.Configuration.WriteTo.Sink(new ErrorWindowSink());
		////this.Configuration.WriteTo.Sink(new ErrorReportingSink());
		this.Configuration.WriteTo.Sink(new DalamudSink(new Formatter(false)));

		this.logger = this.Configuration.CreateLogger();
	}

	public void Information(string message) => this.logger.Information(message);
	public void Information(Exception ex, string message) => this.logger.Information(ex, message);
	public void Warning(string message) => this.logger.Warning(message);
	public void Warning(Exception ex, string message) => this.logger.Warning(ex, message);
	public void Error(string message) => this.logger.Error(message);
	public void Error(Exception ex, string message) => this.logger.Error(ex, message);

	public void Dispose()
	{
	}
}

public class Formatter(bool includeLevel, bool includeContext = true) : ITextFormatter
{
	public void Format(LogEvent logEvent, TextWriter output)
	{
		if (includeLevel)
			output.Write(ToLevelTag(logEvent.Level));
		if (includeContext && logEvent.Properties.TryGetValue("Context", out var contextValue))
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
			output.Write(ex.GetType().Name);
			output.Write(": ");
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

	private static string ToLevelTag(LogEventLevel level)
	{
		switch (level)
		{
			case LogEventLevel.Verbose: return Crayon.Output.Dim().Text("[VRB] ");
			case LogEventLevel.Debug: return Crayon.Output.White().Text("[DBG] ");
			case LogEventLevel.Information: return Crayon.Output.White().Text("[INF] ");
			case LogEventLevel.Warning: return Crayon.Output.Yellow().Text("[WRN] ");
			case LogEventLevel.Error: return Crayon.Output.Red().Text("[ERR] ");
			case LogEventLevel.Fatal: return Crayon.Output.Black().Background.Red().Text("[FAT] ");
		}

		throw new NotSupportedException();
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

public class ErrorReportingSink : ILogEventSink
{
	public void Emit(LogEvent logEvent)
	{
		// TODO
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
				Studio.DalamudLog.Verbose(message);
				break;
			}

			case LogEventLevel.Debug:
			{
				Studio.DalamudLog.Debug(message);
				break;
			}

			case LogEventLevel.Information:
			{
				Studio.DalamudLog.Information(message);
				break;
			}

			case LogEventLevel.Warning:
			{
				Studio.DalamudLog.Warning(message);
				break;
			}

			case LogEventLevel.Error:
			case LogEventLevel.Fatal:
			{
				Studio.DalamudLog.Error(message);
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
		SayHello();
	}

	public void Emit(LogEvent logEvent)
	{
		StringWriter writer = new();
		this.formatter.Format(logEvent, writer);
		string message = writer.ToString();
		message = message.TrimEnd('\r', '\n');

		Debug.WriteLine(message);
	}

	private static void SayHello()
	{
		var rainbow = new Crayon.Rainbow(0.5);
		Debug.WriteLine(rainbow.Next().Text(@"                      @@             _____ _______ _    _ _____ _____ ____		"));
		Debug.WriteLine(rainbow.Next().Text(@"          @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \		"));
		Debug.WriteLine(rainbow.Next().Text(@"         @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |		"));
		Debug.WriteLine(rainbow.Next().Text(@"         @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |		"));
		Debug.WriteLine(rainbow.Next().Text(@"        @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |		"));
		Debug.WriteLine(rainbow.Next().Text(@"    @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/		"));
		Debug.WriteLine(rainbow.Next().Text(@"     @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _	"));
		Debug.WriteLine(rainbow.Next().Text(@"      @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |	"));
		Debug.WriteLine(rainbow.Next().Text(@"      @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |	"));
		Debug.WriteLine(rainbow.Next().Text(@"    @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|	"));
		Debug.WriteLine(rainbow.Next().Text(@"     @@@@             @@@        https://github.com/UnlostWorld/StudioFourteen	"));
		Debug.WriteLine(rainbow.Next().Text(@"       @@@@@      @@@@@															"));
		Debug.WriteLine(rainbow.Next().Text(@"        @@@@@@@@@@@@@@                This software is licensed under the			"));
		Debug.WriteLine(rainbow.Next().Text(@"            @@@@  @                  GNU AFFERO GENERAL PUBLIC LICENSE v3			"));
	}
}