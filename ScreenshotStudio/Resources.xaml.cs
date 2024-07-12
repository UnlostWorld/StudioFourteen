namespace ScreenshotStudio;

using System.Windows;

public partial class Resources : ResourceDictionary
{
	public static Resources? Shared { get; private set; }

	public static Resources Load()
	{
		Resources resources = new();
		resources.Source = new("pack://application:,,,/ScreenshotStudio;component/Resources.xaml");
		return resources;
	}

	public static void LoadShared()
	{
		Shared = Load();
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
		if (Shared == null)
			return fallback;

		if (Shared.Contains(key))
			return Shared[key];

		return fallback;
	}
}
