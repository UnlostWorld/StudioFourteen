namespace ScreenshotStudio;

using System;
using System.Diagnostics;
using Serilog;

public static class UrlUtility
{
	public static void Open(string url)
	{
		try
		{
			Process.Start(new ProcessStartInfo(@"cmd", $"/c start \"\" \"{url}\"") { CreateNoWindow = true });
		}
		catch (Exception ex)
		{
			Logging.Shared.Error(ex, "Failed to navigate to url");
		}
	}
}
