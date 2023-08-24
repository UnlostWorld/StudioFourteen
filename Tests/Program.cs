// © XivTools.
// Licensed under the MIT license.

namespace Tests;

using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Utilities;
using Serilog;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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
				XivWindow.Process = proc;
				foundProcess = true;
				break;
			}
		}

		if (!foundProcess)
		{
			Log.Error("No FFXIV process found");
			return;
		}

		// Test something

		Log.Information("Done!");
	}
}