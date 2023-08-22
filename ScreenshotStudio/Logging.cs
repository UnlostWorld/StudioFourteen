// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio;

using Dalamud.Logging;
using ScreenshotStudio.Studio;
using Serilog;
using Serilog.Events;
using System;

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
		string message = $"[{this.context}] {logEvent.MessageTemplate.Text}";

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