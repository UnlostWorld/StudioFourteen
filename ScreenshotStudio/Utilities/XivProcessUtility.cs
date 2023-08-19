namespace ScreenshotStudio.Utilities;

using System;
using System.Diagnostics;

public static class XivProcessUtility
{
	private static Process? process;

	public static Process Process
	{
		get
		{
			if (process == null)
				process = Process.GetCurrentProcess();

			return process;
		}

		set => process = value;
	}
}
