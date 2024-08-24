namespace ScreenshotStudio.Studio.Settings;

using ScreenshotStudio.Input;

using System;
using WpfUtils.Converters;

public class KeyBindEventsToStringConverter : ConverterBase<KeyBindEvents, string>
{
	protected override string Convert(KeyBindEvents value)
	{
		return Resources.Find($"LOC_Settings_Input_{value.ToString()}", value.ToString());
	}
}
