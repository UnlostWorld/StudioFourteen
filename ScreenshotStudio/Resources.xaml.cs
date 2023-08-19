namespace ScreenshotStudio;

using System;
using System.Windows;

public partial class Resources : ResourceDictionary
{
	protected override void OnGettingValue(object key, ref object value, out bool canCache)
	{
		base.OnGettingValue(key, ref value, out canCache);
	}

	private static Resources? instance;

	public static Resources Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new();
				instance.Source = new("pack://application:,,,/ScreenshotStudio;component/Resources.xaml");
			}

			return instance;
		}
	}
}
