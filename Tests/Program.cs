namespace Tests;

using ScreenshotStudio.Utilities;
using ScreenshotStudio.Windows;
using Serilog;
using System.Diagnostics;

internal class Program
{
	static void Main(string[] args)
	{
		LoggerConfiguration config = new LoggerConfiguration();
		config.WriteTo.Console();
		config.WriteTo.Debug();

		Log.Logger = config.CreateLogger();

		Log.Information("Starting...");

		bool foundProcess = false;
		foreach(Process proc in Process.GetProcesses())
		{
			if (proc.ProcessName.ToLower().Contains("ffxiv_dx11"))
			{
				XivProcessUtility.Process = proc;
				foundProcess = true;
				break;
			}
		}

		if (!foundProcess)
		{
			Log.Error("No FFXIV process found");
			return;
		}

		Window1.Show();

		Log.Information("Done");
	}
}