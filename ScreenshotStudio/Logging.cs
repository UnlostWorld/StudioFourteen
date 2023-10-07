namespace ScreenshotStudio;

using Dalamud.Logging;
using ScreenshotStudio.Studio;
using Serilog;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

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
		WpfUtils.Logging.Log.HandleMessage = XivtoolsWpfLog;
		WpfUtils.Logging.Log.HandleError = XivtoolsWpfError;
	}

	public static void XivtoolsWpfLog(string message) => Shared.Information(message);
	public static void XivtoolsWpfError(Exception ex, string message) => Shared.Error(ex, message);
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
				PluginLog.Verbose(logEvent.Exception, message);
				break;
			}

			case LogEventLevel.Debug:
			{
				PluginLog.Debug(logEvent.Exception, message);
				break;
			}

			case LogEventLevel.Information:
			{
				PluginLog.Information(logEvent.Exception, message);
				break;
			}

			case LogEventLevel.Warning:
			{
				PluginLog.Warning(logEvent.Exception, message);
				break;
			}

			case LogEventLevel.Error:
			case LogEventLevel.Fatal:
			{
				PluginLog.Error(logEvent.Exception, message);
				ErrorWindow.Show(logEvent.MessageTemplate.Text);
				break;
			}
		}
	}
}