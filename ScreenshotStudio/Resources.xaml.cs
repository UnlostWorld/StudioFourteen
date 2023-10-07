namespace ScreenshotStudio;

using System.Windows;

public partial class Resources : ResourceDictionary
{
	public static Resources Load()
	{
		Resources resources = new();
		resources.Source = new("pack://application:,,,/ScreenshotStudio;component/Resources.xaml");
		return resources;
	}

	public static object? Find(object key, object? fallback = null)
	{
		return fallback;
	}
}
