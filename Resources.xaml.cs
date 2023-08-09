namespace ScreenshotStudio
{
	using Dalamud.Logging;
	using System.Windows;

	public partial class Resources : ResourceDictionary
	{
		protected override void OnGettingValue(object key, ref object value, out bool canCache)
		{
			base.OnGettingValue(key, ref value, out canCache);
		}
	}
}
