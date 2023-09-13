// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Utilities;

using ScreenshotStudio.Services;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using XivToolsWpf;
using XivToolsWpf.Utility;

public static class GamerRGBUtility
{
	public static void RGBarf(FrameworkElement target, string resourceName = "TrimBrush")
	{
		Task.Run(async () => await RGBarfTask(target, resourceName));
	}

	private static async Task RGBarfTask(FrameworkElement target, string resourceName = "TrimBrush")
	{
		try
		{
			int brushCount = 256;

			SolidColorBrush[] brushes = new SolidColorBrush[brushCount];

			for (int i = 0; i < brushCount; i++)
			{
				double h = i / (double)brushCount;
				brushes[i] = new SolidColorBrush(ColorUtility.FromHsl(h, 1, 0.5));
				brushes[i].Freeze();
			}

			int current = 0;
			while (ServiceManager.Instance != null && ServiceManager.Instance.Panels.IsAlive)
			{
				await Task.Delay(33);
				await target.Dispatcher.MainThread();

				current++;
				if (current >= brushCount)
					current = 0;

				target.Resources[resourceName] = brushes[current];
			}
		}
		catch (Exception ex)
		{
			Logging.Shared.Error(ex, "oh no");
		}
	}
}
