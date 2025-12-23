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
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Serilog.Parsing;
using System.Collections.Generic;

public static class Logging
{
	public static readonly LoggerConfiguration Configuration;
	private static readonly ILogger Logger;

	static Logging()
	{
		Configuration = new LoggerConfiguration();
		Configuration.Enrich.With<StackEnricher>();
		Configuration.WriteTo.Sink(new DebugSink(new Formatter(true)));
		////Configuration.WriteTo.Sink(new ErrorWindowSink());
		Configuration.WriteTo.Sink(new ErrorReportingSink());
		Configuration.WriteTo.Sink(new DalamudSink(new Formatter(false)));

		Logger = Configuration.CreateLogger();

		StudioTraceListener listener = new();
	}

	public static ILogger Shared => Logger;

	public static ILogger ForContext<T>() => ForContext(typeof(T));
	public static ILogger ForContext(Type type) => ForContext(type.Name);

	public static ILogger ForContext(string context)
	{
		return Logger.ForContext("Context", context);
	}

	public static void Information(string message) => Shared.Information(message);

	public static void Dispose()
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
			case LogEventLevel.Verbose: return "[VRB] ";
			case LogEventLevel.Debug: return "[DBG] ";
			case LogEventLevel.Information: return "[INF] ";
			case LogEventLevel.Warning: return "[WRN] ";
			case LogEventLevel.Error: return "[ERR] ";
			case LogEventLevel.Fatal: return "[FAT] ";
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
		if (ServiceManager.ShutdownRequested)
			return;

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
	public static bool IsWriting = false;
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
		message = message.TrimEnd('\r', '\n');

		IsWriting = true;
		Debug.WriteLine(message);
		IsWriting = false;
	}
}

public class StudioTraceListener
	: TraceListener
{
	public override void Write(string? message)
	{
	}

	public override void WriteLine(string? message)
	{
	}

	public override void TraceEvent(TraceEventCache? eventCache, string source, TraceEventType eventType, int id, [StringSyntax("CompositeFormat")] string? format, params object?[]? args)
	{
		base.TraceEvent(eventCache, source, eventType, id, format, args);

		if (format == null)
			return;

		if (eventType <= TraceEventType.Warning)
			return;

		string message = format;

		if (args != null)
			message = string.Format(CultureInfo.InvariantCulture, format!, args);

		MessageTemplate template = new MessageTemplateParser().Parse(message);
		List<LogEventProperty> properties = new()
		{
			new LogEventProperty("Context", new ScalarValue(source)),
		};

		LogEvent evt = new(DateTimeOffset.Now, ToSerilogLevel(eventType), null, template, properties);
		Logging.Shared.Write(evt);
	}

	private static LogEventLevel ToSerilogLevel(TraceEventType eventType)
	{
		switch (eventType)
		{
			case TraceEventType.Critical: return LogEventLevel.Fatal;
			case TraceEventType.Error: return LogEventLevel.Error;
			case TraceEventType.Warning: return LogEventLevel.Warning;
			case TraceEventType.Information: return LogEventLevel.Information;
			case TraceEventType.Verbose:
			case TraceEventType.Start:
			case TraceEventType.Stop:
			case TraceEventType.Suspend:
			case TraceEventType.Resume:
			case TraceEventType.Transfer: return LogEventLevel.Verbose;
		}

		throw new NotSupportedException();
	}
}