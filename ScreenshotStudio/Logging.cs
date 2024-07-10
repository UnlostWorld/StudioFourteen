namespace ScreenshotStudio;

using ScreenshotStudio.Plugin;
using ScreenshotStudio.Studio;
using Serilog;
using Serilog.Events;
using System;
using System.Diagnostics;

public static class Logging
{
	private static ILogger? shared;

	public static ILogger Shared
	{
		get
		{
			if (shared == null)
				shared = ForContext(string.Empty);

			return shared;
		}
	}

	public static ILogger ForContext<T>() => ForContext(typeof(T));
	public static ILogger ForContext(Type type) => ForContext(type.Name);

	public static ILogger ForContext(string context)
	{
		return new Logger(context);
	}

	public static void Init()
	{
		WpfUtils.Logging.Log.HandleMessage = WpfLog;
		WpfUtils.Logging.Log.HandleError = WpfError;
	}

	public static void WpfLog(string message) => Shared.Information(message);
	public static void WpfError(Exception ex, string message) => Shared.Error(ex, message);

	public static void Information(string message) => Shared.Information(message);
}

public class Logger : ILogger
{
	private readonly string context;

	public Logger(string context)
	{
		this.context = context;
	}

	public void Write(LogEvent logEvent)
	{
		string message;
		if (logEvent.Level > LogEventLevel.Warning || logEvent.Exception != null)
		{
			// Include a stack trace for warnings or above or events with an exception from where the log originated.
			StackTrace stack = new(3, true);
			message = $"[{this.context}] {logEvent.MessageTemplate.Text} \n\nfrom:\n{stack}";
		}
		else
		{
			message = $"[{this.context}] {logEvent.MessageTemplate.Text}";
		}

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
				ErrorWindow.Show(logEvent.MessageTemplate.Text);
				break;
			}
		}
	}
}