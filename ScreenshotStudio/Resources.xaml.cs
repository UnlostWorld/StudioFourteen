namespace ScreenshotStudio;

using System;
using System.Windows;

public partial class Resources : ResourceDictionary
{
	public static Resources Load()
	{
		Resources resources = new();
		resources.Source = new("pack://application:,,,/ScreenshotStudio;component/Resources.xaml");
		return resources;
	}
}
