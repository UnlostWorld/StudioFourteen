namespace ScreenshotStudio.Utilities;

using ScreenshotStudio.Plugin;
using System;
using System.Threading.Tasks;

public static class Threads
{
	public static bool IsFrameworkThread => DalamudServices.Framework?.IsInFrameworkUpdateThread == true;
	public static Task RunOnFrameworkThread(Action action)
	{
		if (!IsFrameworkThread)
		{
			return DalamudServices.Framework?.RunOnFrameworkThread(action) ?? Task.CompletedTask;
		}

		action.Invoke();
		return Task.CompletedTask;
	}

	public static void VerifyFrameworkThread()
	{
		if (!IsFrameworkThread)
		{
			throw new InvalidThreadException();
		}
	}
}

public class InvalidThreadException : Exception
{
	public InvalidThreadException()
		: base("Invalid Thread")
	{
	}
}
