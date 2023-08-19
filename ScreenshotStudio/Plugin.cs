namespace ScreenshotStudio;

using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using ScreenshotStudio.Windows;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using XivToolsWpf.Dialogs;

public sealed class Plugin : IDalamudPlugin
{
	public string Name => "Screenshot Studio";

	[PluginService][RequiredVersion("1.0")] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
	[PluginService][RequiredVersion("1.0")] public static CommandManager CommandManager { get; private set; } = null!;
	[PluginService][RequiredVersion("1.0")] public static ChatGui ChatGui { get; private set; } = null!;
	[PluginService][RequiredVersion("1.0")] public static SigScanner SigScanner { get; private set; } = null!;

	Window1? wnd;

	public Plugin()
	{
		Task.Run(this.Start);
	}

	public static Process XivProcess => Process.GetCurrentProcess();
	public ILogger Log => Serilog.Log.ForContext<Plugin>();

	public void Dispose()
	{
		this.wnd?.Close();
	}

	private async Task Start()
	{
		this.wnd = await Window1.ShowAsync();

		/*Stopwatch sw = new Stopwatch();
		sw.Start();
		
		try
		{
			LoggerConfiguration config = new LoggerConfiguration();
			config.WriteTo.Sink<XlLogDestination>();
			config.WriteTo.Debug();

			Serilog.Log.Logger = config.CreateLogger();

			this.Log.Information("Starting...");

			if (Application.ResourceAssembly == null)
			{
				Application.ResourceAssembly = Assembly.GetExecutingAssembly();
				PluginLog.Information($"Changing resource assembly to: {Application.ResourceAssembly}");
			}

			await Services.InitializeCriticalServices();
			await Services.InitializeServices();
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to start");
			PluginLog.Error(ex, "Failed to start");
		}

		sw.Stop();
		Log.Information($"Started in {sw.ElapsedMilliseconds}ms");*/
	}

	private class XlLogDestination : ILogEventSink
	{
		public void Emit(LogEvent logEvent)
		{
			if (logEvent.Level >= LogEventLevel.Fatal)

			if (logEvent.Level >= LogEventLevel.Error)
			{
				ErrorDialog.ShowError(ExceptionDispatchInfo.Capture(new Exception(logEvent.MessageTemplate.Text, logEvent.Exception)), logEvent.Level == LogEventLevel.Fatal);
			}

			switch (logEvent.Level)
			{
				case LogEventLevel.Verbose:
				{
					PluginLog.Verbose(logEvent.Exception, logEvent.RenderMessage());
					break;
				}

				case LogEventLevel.Debug:
				{
					PluginLog.Debug(logEvent.Exception, logEvent.RenderMessage());
					break;
				}

				case LogEventLevel.Information:
				{
					PluginLog.Information(logEvent.Exception, logEvent.RenderMessage());
					break;
				}

				case LogEventLevel.Warning:
				{
					PluginLog.Warning(logEvent.Exception, logEvent.RenderMessage());
					break;
				}

				case LogEventLevel.Error:
				case LogEventLevel.Fatal:
				{
					PluginLog.Error(logEvent.Exception, logEvent.RenderMessage());
					break;
				}
			}
		}
	}
}
