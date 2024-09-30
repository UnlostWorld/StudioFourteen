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
using System.Threading.Tasks;

public static class Logging
{
	public static readonly LoggerConfiguration Configuration;
	private static readonly ILogger Logger;

	static Logging()
	{
		Formatter formatter = new();

		Configuration = new LoggerConfiguration();
		Configuration.Enrich.With<StackEnricher>();
		Configuration.WriteTo.Debug(formatter);
		Configuration.WriteTo.Sink(new ErrorWindowSink());
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

		output.Write(logEvent.MessageTemplate);

		if (logEvent.Properties.TryGetValue("StackTrace", out var stackTrace))
		{
			output.WriteLine();
			if (stackTrace is ScalarValue sv)
			{
				output.Write(sv.Value);
			}
		}

		if (logEvent.Exception != null)
		{
			output.WriteLine();
			output.WriteLine(logEvent.Exception.Message);
			output.WriteLine(logEvent.Exception.StackTrace);
		}

		output.WriteLine();
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

public class ErrorWindowSink : ILogEventSink
{
	public void Emit(LogEvent logEvent)
	{
		if (logEvent.Level >= LogEventLevel.Error)
		{
			ErrorWindow.Show(logEvent.MessageTemplate.Text);
		}
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
				DalamudServices.Log?.Verbose(logEvent.Exception, message);
				break;
			}

			case LogEventLevel.Debug:
			{
				DalamudServices.Log?.Debug(logEvent.Exception, message);
				break;
			}

			case LogEventLevel.Information:
			{
				DalamudServices.Log?.Information(logEvent.Exception, message);
				break;
			}

			case LogEventLevel.Warning:
			{
				DalamudServices.Log?.Warning(logEvent.Exception, message);
				break;
			}

			case LogEventLevel.Error:
			case LogEventLevel.Fatal:
			{
				DalamudServices.Log?.Error(logEvent.Exception, message);
				break;
			}
		}
	}
}