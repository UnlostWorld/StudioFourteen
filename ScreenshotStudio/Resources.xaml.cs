namespace ScreenshotStudio;

using System.Windows;

public partial class Resources : ResourceDictionary
{
	private static readonly Resources Instance = Load();

	public static Resources Load()
	{
		Resources resources = new();
		resources.Source = new("pack://application:,,,/ScreenshotStudio;component/Resources.xaml");
		return resources;
	}

	public static string Find(object key, string fallback)
	{
		object? val = Find(key, (object?)fallback);

		if (val is string valStr)
			return valStr;

		return fallback;
	}

	public static object? Find(object key, object? fallback = null)
	{
		if (Instance.Contains(key))
			return Instance[key];

		return fallback;
	}
}
