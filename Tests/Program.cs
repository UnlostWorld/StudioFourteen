namespace Tests;

using ScreenshotStudio.Windows;
using Serilog;

internal class Program
{
	static void Main(string[] args)
	{
		Console.WriteLine("Hello, World!");

		LoggerConfiguration config = new LoggerConfiguration();
		config.WriteTo.Console();
		config.WriteTo.Debug();

		Serilog.Log.Logger = config.CreateLogger();

		Task.Run(async () =>
		{
			Window1? wnd = await Window1.CreateInstance<Window1>();
			wnd?.Show();

		}).Wait();
	}
}